using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PS3Sharp.Backends.RPCS3
{
    internal class MacProcessMemory : IProcessMemory
    {
        private Process? _process;
        private uint _taskPort;
        private bool _disposed;

        // mach kernel calls
        [DllImport("libSystem.dylib")]
        private static extern int task_for_pid(uint targetTask, int pid, out uint task);

        [DllImport("libSystem.dylib")]
        private static extern int mach_task_self();

        [DllImport("libSystem.dylib")]
        private static extern int mach_vm_read_overwrite(uint targetTask, ulong address, ulong size, ulong data, out ulong outSize);

        [DllImport("libSystem.dylib")]
        private static extern int mach_vm_write(uint targetTask, ulong address, IntPtr data, uint dataCnt);

        [DllImport("libSystem.dylib")]
        private static extern int mach_vm_protect(uint targetTask, ulong address, ulong size, int setMaximum, int newProtection);

        private const int KERN_SUCCESS = 0;
        private const int VM_PROT_READ = 0x01;
        private const int VM_PROT_WRITE = 0x02;

        public bool IsAttached => _taskPort != 0;

        public bool Attach(string processName)
        {
            _process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (_process == null)
                return false;

            int result = task_for_pid((uint)mach_task_self(), _process.Id, out _taskPort);

            if (result != KERN_SUCCESS || _taskPort == 0)
                throw new UnauthorizedAccessException(
                    $"[RPCS3] - task_for_pid failed for '{processName}' (error {result}). " +
                    "Try running with sudo or sign your app with the com.apple.security.cs.debugger entitlement.");

            return true;
        }

        public void Detach()
        {
            _taskPort = 0;

            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }
        }

        public bool Read(IntPtr address, byte[] buffer, int length, out int bytesRead)
        {
            bytesRead = 0;
            if (_taskPort == 0)
                return false;

            // pin the buffer and read directly into it
            mach_vm_protect(_taskPort, (ulong)address, (ulong)length, 0, VM_PROT_READ);

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                int result = mach_vm_read_overwrite(
                    _taskPort,
                    (ulong)address,
                    (ulong)length,
                    (ulong)handle.AddrOfPinnedObject(),
                    out ulong outSize);

                bytesRead = (int)outSize;
                return result == KERN_SUCCESS;
            }
            finally
            {
                handle.Free();
            }
        }

        public bool Write(IntPtr address, byte[] data, int length, out int bytesWritten)
        {
            bytesWritten = 0;
            if (_taskPort == 0)
                return false;

            // try mach_vm_protect first to ensure write access
            mach_vm_protect(_taskPort, (ulong)address, (ulong)length, 0, VM_PROT_READ | VM_PROT_WRITE);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                int result = mach_vm_write(
                    _taskPort,
                    (ulong)address,
                    handle.AddrOfPinnedObject(),
                    (uint)length);

                if (result == KERN_SUCCESS)
                    bytesWritten = length;

                return result == KERN_SUCCESS;
            }
            finally
            {
                handle.Free();
            }
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
