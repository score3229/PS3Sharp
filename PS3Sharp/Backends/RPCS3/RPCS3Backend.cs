using System.Diagnostics;
using System.Runtime.InteropServices;
using PS3Sharp.Types;

namespace PS3Sharp.Backends.RPCS3
{
    public class RPCS3Backend : PS3BackendBase
    {
        private IProcessMemory _memory;
        private string _processName;
        private ulong _baseAddress;
        private bool _disposed;

        public override BackendType Type => BackendType.RPCS3;
        public override bool IsConnected => _memory.IsAttached;

        public RPCS3Backend(string processName = "rpcs3")
        {
            _processName = processName;
            _memory = CreatePlatformMemory();
        }

        private static IProcessMemory CreatePlatformMemory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsProcessMemory();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new LinuxProcessMemory();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new MacProcessMemory();

            throw new PlatformNotSupportedException(
                $"RPCS3 backend is not supported on {RuntimeInformation.OSDescription}.");
        }

        public override bool Connect()
        {
            try
            {
                if (!_memory.Attach(_processName))
                    return false;

                _baseAddress = DiscoverBaseAddress();
                return _baseAddress != 0;
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect() => _memory.Detach();

        // ps3 address -> emulator virtual address
        // RPCS3 maps PS3 memory at g_base_addr (0x300000000) as protected
        // and an unprotected mirror at g_base_addr + 0x100000000 (0x400000000)
        // we always use the mirror for read/write access
        private IntPtr MapAddress(uint address)
        {
            return (IntPtr)(_baseAddress + 0x100000000UL + address);
        }

        // discover g_base_addr from RPCS3's log file
        // RPCS3 prints "vm::g_base_addr = XXXXXXXXXXXXXXXX" on startup
        private ulong DiscoverBaseAddress()
        {
            var logPaths = GetLogPaths();

            foreach (var path in logPaths)
            {
                if (!File.Exists(path))
                    continue;

                ulong lastMatch = 0;
                foreach (var line in File.ReadLines(path))
                {
                    int idx = line.IndexOf("vm::g_base_addr = ");
                    if (idx < 0) continue;

                    string hex = "";
                    int start = idx + "vm::g_base_addr = ".Length;
                    for (int i = start; i < line.Length; i++)
                    {
                        if (Uri.IsHexDigit(line[i]))
                            hex += line[i];
                        else
                            break;
                    }

                    if (ulong.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out ulong addr))
                        lastMatch = addr;
                }

                if (lastMatch != 0)
                    return lastMatch;
            }

            // fallback: 0x300000000 is the standard base on Windows and Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return 0x300000000UL;

            throw new InvalidOperationException(
                "[RPCS3] - Could not discover base address from RPCS3.log. Make sure RPCS3 is running with a game loaded.");
        }

        private static List<string> GetLogPaths()
        {
            var paths = new List<string>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                paths.Add(Path.Combine(appData, "rpcs3", "RPCS3.log"));
                // portable installs: log next to the exe
                var procs = Process.GetProcessesByName("rpcs3");
                if (procs.Length > 0 && procs[0].MainModule != null)
                {
                    string? dir = Path.GetDirectoryName(procs[0].MainModule!.FileName);
                    if (dir != null)
                        paths.Add(Path.Combine(dir, "RPCS3.log"));
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string? sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
                if (sudoUser != null)
                    home = $"/home/{sudoUser}";
                paths.Add(Path.Combine(home, ".config", "rpcs3", "RPCS3.log"));
                paths.Add(Path.Combine(home, ".cache", "rpcs3", "RPCS3.log"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                // sudo changes home dir, check SUDO_USER
                string? sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
                if (sudoUser != null)
                    paths.Add($"/Users/{sudoUser}/Library/Caches/rpcs3/RPCS3.log");
                paths.Add(Path.Combine(home, "Library", "Caches", "rpcs3", "RPCS3.log"));
                paths.Add(Path.Combine(home, ".config", "rpcs3", "RPCS3.log"));
            }

            return paths;
        }

        protected override byte[] ReadMemory(uint address, int length)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[RPCS3] - Not connected.");

            byte[] buffer = new byte[length];
            bool success = _memory.Read(MapAddress(address), buffer, length, out int bytesRead);

            if (!success || bytesRead != length)
                throw new InvalidOperationException($"[RPCS3] - Failed to read {length} bytes at 0x{address:X} (mapped: 0x{(ulong)MapAddress(address):X}).");

            return buffer;
        }

        protected override void WriteMemory(uint address, byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[RPCS3] - Not connected.");

            bool success = _memory.Write(MapAddress(address), data, data.Length, out int bytesWritten);

            if (!success || bytesWritten != data.Length)
                throw new InvalidOperationException($"[RPCS3] - Failed to write {data.Length} bytes at 0x{address:X} (mapped: 0x{(ulong)MapAddress(address):X}).");
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _memory.Dispose();
                _disposed = true;
            }
        }

        public override string ToString() => $"[RPCS3] - Connected: {IsConnected}";
    }
}
