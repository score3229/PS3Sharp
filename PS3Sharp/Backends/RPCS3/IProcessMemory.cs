namespace PS3Sharp.Backends.RPCS3
{
    internal interface IProcessMemory : IDisposable
    {
        bool Attach(string processName);
        void Detach();
        bool IsAttached { get; }
        bool Read(IntPtr address, byte[] buffer, int length, out int bytesRead);
        bool Write(IntPtr address, byte[] data, int length, out int bytesWritten);
    }
}
