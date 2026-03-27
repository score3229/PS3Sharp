using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PS3Sharp.Backends.RPCS3
{
    internal class WindowsProcessMemory : IProcessMemory
    {
        private Process? _process;
        private IntPtr _handle;
        private bool _disposed;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;

        public bool IsAttached => _handle != IntPtr.Zero;

        public bool Attach(string processName)
        {
            _process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (_process == null)
                return false;

            _handle = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _process.Id);
            return _handle != IntPtr.Zero;
        }

        public void Detach()
        {
            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }

            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }
        }

        public bool Read(IntPtr address, byte[] buffer, int length, out int bytesRead)
        {
            return ReadProcessMemory(_handle, address, buffer, length, out bytesRead);
        }

        public bool Write(IntPtr address, byte[] data, int length, out int bytesWritten)
        {
            return WriteProcessMemory(_handle, address, data, length, out bytesWritten);
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
