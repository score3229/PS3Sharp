using System.Runtime.InteropServices;
#pragma warning disable CA1416 // platform compatibility (guarded by IsAvailable)
using Microsoft.Win32;

namespace PS3Sharp.Backends.CCAPI
{
    internal class CCAPIDllConnection : IDisposable
    {
        private IntPtr _libHandle;
        private uint _processId;
        private bool _disposed;

        // delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ConnectConsoleDelegate(string targetIP);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DisconnectConsoleDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetConnectionStatusDelegate(ref int status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetMemoryDelegate(uint pid, ulong addr, uint size, byte[] data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetMemoryDelegate(uint pid, ulong addr, uint size, byte[] data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetProcessListDelegate(ref uint count, IntPtr pids);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetProcessNameDelegate(uint pid, IntPtr name);

        private ConnectConsoleDelegate? _connect;
        private DisconnectConsoleDelegate? _disconnect;
        private GetConnectionStatusDelegate? _getStatus;
        private SetMemoryDelegate? _setMemory;
        private GetMemoryDelegate? _getMemory;
        private GetProcessListDelegate? _getProcessList;
        private GetProcessNameDelegate? _getProcessName;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string name);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr module);

        internal bool IsConnected
        {
            get
            {
                if (_getStatus == null) return false;
                int status = 0;
                _getStatus(ref status);
                return status != 0;
            }
        }

        internal static bool IsAvailable()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            return FindDllPath() != null;
        }

        private static string? FindDllPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\FrenchModdingTeam\CCAPI\InstallFolder");
                if (key != null)
                {
                    string? path = key.GetValue("path") as string;
                    if (path != null)
                    {
                        string dll = Path.Combine(path, "CCAPI.dll");
                        if (File.Exists(dll))
                            return dll;
                    }
                }
            }
            catch { }

            return null;
        }

        internal bool Connect(string ipAddress)
        {
            string? dllPath = FindDllPath();
            if (dllPath == null)
                throw new InvalidOperationException("[CCAPI] - DLL path not found in registry.");

            _libHandle = LoadLibrary(dllPath);
            if (_libHandle == IntPtr.Zero)
                throw new InvalidOperationException($"[CCAPI] - Failed to load {dllPath}. The DLL may be 32-bit (requires x86 build).");

            _connect = GetFunc<ConnectConsoleDelegate>("CCAPIConnectConsole");
            _disconnect = GetFunc<DisconnectConsoleDelegate>("CCAPIDisconnectConsole");
            _getStatus = GetFunc<GetConnectionStatusDelegate>("CCAPIGetConnectionStatus");
            _setMemory = GetFunc<SetMemoryDelegate>("CCAPISetMemory");
            _getMemory = GetFunc<GetMemoryDelegate>("CCAPIGetMemory");
            _getProcessList = GetFunc<GetProcessListDelegate>("CCAPIGetProcessList");
            _getProcessName = GetFunc<GetProcessNameDelegate>("CCAPIGetProcessName");

            if (_connect == null || _setMemory == null || _getMemory == null || _getProcessList == null)
                throw new InvalidOperationException("[CCAPI] - Failed to resolve DLL exports.");

            int result = _connect(ipAddress);
            if (result != 0)
                throw new InvalidOperationException($"[CCAPI] - ConnectConsole returned error {result}.");

            return true;
        }

        internal bool AttachGameProcess()
        {
            if (_getProcessList == null || _getProcessName == null)
                return false;

            IntPtr pidsPtr = Marshal.AllocHGlobal(sizeof(uint) * 64);
            try
            {
                uint count = 64;
                int res = _getProcessList(ref count, pidsPtr);
                if (res != 0 || count == 0)
                    return false;

                IntPtr namePtr = Marshal.AllocHGlobal(512);
                try
                {
                    for (uint i = 0; i < count; i++)
                    {
                        uint pid = (uint)Marshal.ReadInt32(pidsPtr, (int)(i * sizeof(uint)));
                        if (pid == 0) continue;

                        _getProcessName(pid, namePtr);
                        string? name = Marshal.PtrToStringAnsi(namePtr);

                        if (name != null && !name.Contains("dev_flash"))
                        {
                            _processId = pid;
                            return true;
                        }
                    }
                }
                finally { Marshal.FreeHGlobal(namePtr); }
            }
            finally { Marshal.FreeHGlobal(pidsPtr); }

            return false;
        }

        internal void Disconnect()
        {
            if (_libHandle == IntPtr.Zero)
                return;

            try { _disconnect?.Invoke(); } catch { }

            // don't free the library — CCAPI.dll crashes on unload
            // the OS will clean it up on process exit
            _libHandle = IntPtr.Zero;
            _disconnect = null;
            _connect = null;
            _getStatus = null;
            _setMemory = null;
            _getMemory = null;
            _getProcessList = null;
            _getProcessName = null;
        }

        internal byte[] ReadMemory(uint address, int length)
        {
            if (_getMemory == null)
                throw new InvalidOperationException("[CCAPI] - DLL not loaded.");

            byte[] buffer = new byte[length];
            int res = _getMemory(_processId, address, (uint)length, buffer);

            if (res != 0)
                throw new InvalidOperationException($"[CCAPI] - Read failed at 0x{address:X}, result: {res}.");

            return buffer;
        }

        internal void WriteMemory(uint address, byte[] data)
        {
            if (_setMemory == null)
                throw new InvalidOperationException("[CCAPI] - DLL not loaded.");

            int res = _setMemory(_processId, address, (uint)data.Length, data);

            if (res != 0)
                throw new InvalidOperationException($"[CCAPI] - Write failed at 0x{address:X}, result: {res}.");
        }

        private T? GetFunc<T>(string name) where T : Delegate
        {
            IntPtr ptr = GetProcAddress(_libHandle, name);
            if (ptr == IntPtr.Zero) return null;
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
        }
    }
}
