using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PS3Sharp.Backends.RPCS3
{
    internal class LinuxProcessMemory : IProcessMemory
    {
        private int _fd = -1;
        private Process? _process;
        private bool _disposed;

        [DllImport("libc", SetLastError = true)]
        private static extern int open([MarshalAs(UnmanagedType.LPStr)] string path, int flags);

        [DllImport("libc", SetLastError = true)]
        private static extern long pread(int fd, byte[] buf, ulong count, long offset);

        [DllImport("libc", SetLastError = true)]
        private static extern long pwrite(int fd, byte[] buf, ulong count, long offset);

        [DllImport("libc")]
        private static extern int close(int fd);

        private const int O_RDWR = 2;

        public bool IsAttached => _fd >= 0;

        public bool Attach(string processName)
        {
            // try AppRun.wrapped first (RPCS3 AppImage — the emulation process with most threads)
            _process = FindAppRunWrapped();

            // then try exact name match
            if (_process == null)
                _process = Process.GetProcessesByName(processName).FirstOrDefault();

            // last resort: search cmdline for the name
            if (_process == null)
                _process = FindByCommandLine(processName);

            if (_process == null)
                return false;

            string memPath = $"/proc/{_process.Id}/mem";
            _fd = open(memPath, O_RDWR);

            if (_fd < 0)
                throw new UnauthorizedAccessException(
                    $"[RPCS3] - Failed to open {memPath}. Try running with sudo.");

            return true;
        }

        // find process by searching /proc/*/cmdline
        private static Process? FindByCommandLine(string name)
        {
            return Process.GetProcesses().FirstOrDefault(p =>
            {
                try
                {
                    string cmdline = File.ReadAllText($"/proc/{p.Id}/cmdline");
                    return cmdline.Contains(name, StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            });
        }

        // find AppRun.wrapped process with the most threads (RPCS3 emulation process)
        private static Process? FindAppRunWrapped()
        {
            Process? best = null;
            int bestThreads = 0;

            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    string comm = File.ReadAllText($"/proc/{p.Id}/comm").Trim();
                    if (comm != "AppRun.wrapped")
                        continue;

                    int threads = p.Threads.Count;
                    if (threads > bestThreads)
                    {
                        bestThreads = threads;
                        best = p;
                    }
                }
                catch { }
            }

            return best;
        }

        public void Detach()
        {
            if (_fd >= 0)
            {
                close(_fd);
                _fd = -1;
            }

            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }
        }

        public bool Read(IntPtr address, byte[] buffer, int length, out int bytesRead)
        {
            bytesRead = 0;
            if (_fd < 0) return false;

            long result = pread(_fd, buffer, (ulong)length, (long)address);
            if (result < 0) return false;

            bytesRead = (int)result;
            return bytesRead == length;
        }

        public bool Write(IntPtr address, byte[] data, int length, out int bytesWritten)
        {
            bytesWritten = 0;
            if (_fd < 0) return false;

            long result = pwrite(_fd, data, (ulong)length, (long)address);
            if (result < 0) return false;

            bytesWritten = (int)result;
            return bytesWritten == length;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Detach();
                _disposed = true;
            }
        }
    }
}
