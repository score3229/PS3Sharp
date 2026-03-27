using PS3Sharp.Types;

namespace PS3Sharp.Backends.CCAPI
{
    public class CCAPIBackend : PS3BackendBase
    {
        private CCAPIDllConnection? _dllConnection;
        private readonly string _ipAddress;
        private bool _disposed;

        public override BackendType Type => BackendType.CCAPI;
        public override bool IsConnected => _dllConnection?.IsConnected ?? false;

        public CCAPIBackend(string ipAddress)
        {
            _ipAddress = ipAddress;
        }

        public override bool Connect()
        {
            try
            {
                if (!CCAPIDllConnection.IsAvailable())
                    throw new PlatformNotSupportedException(
                        "[CCAPI] - CCAPI.dll not found. Install CCAPI on Windows, or use MAPI as a cross-platform alternative.");

                _dllConnection = new CCAPIDllConnection();
                if (!_dllConnection.Connect(_ipAddress))
                    return false;

                return _dllConnection.AttachGameProcess();
            }
            catch (PlatformNotSupportedException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect() => _dllConnection?.Disconnect();

        protected override byte[] ReadMemory(uint address, int length)
        {
            if (!IsConnected || _dllConnection == null)
                throw new InvalidOperationException("[CCAPI] - Not connected.");

            return _dllConnection.ReadMemory(address, length);
        }

        protected override void WriteMemory(uint address, byte[] data)
        {
            if (!IsConnected || _dllConnection == null)
                throw new InvalidOperationException("[CCAPI] - Not connected.");

            _dllConnection.WriteMemory(address, data);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _dllConnection?.Dispose();
                _disposed = true;
            }
        }

        public override string ToString() => $"[CCAPI] - {_ipAddress} - Connected: {IsConnected}";
    }
}
