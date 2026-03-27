using PS3Sharp.Types;

namespace PS3Sharp.Backends.TMAPI
{
    public class TMAPIBackend : PS3BackendBase
    {
        private TMAPIConnection? _directConnection;
        private TMAPIDllConnection? _dllConnection;
        private readonly string _ipAddress;
        private readonly int _port;
        private bool _useDll;
        private bool _disposed;

        private const int MAX_CHUNK_SIZE = 2048;

        public override BackendType Type => BackendType.TMAPI;
        public override bool IsConnected => _useDll
            ? (_dllConnection?.IsConnected ?? false)
            : (_directConnection?.IsConnected ?? false);

        public TMAPIBackend(string ipAddress, int port = TMAPIConnection.DEFAULT_PORT)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public override bool Connect()
        {
            try
            {
                // if Target Manager is installed, use the DLL (works alongside ProDG)
                if (TMAPIDllConnection.IsAvailable())
                {
                    _dllConnection = new TMAPIDllConnection();
                    if (_dllConnection.Connect())
                    {
                        _dllConnection.AttachProcess();
                        _useDll = true;
                        return true;
                    }
                    _dllConnection.Dispose();
                    _dllConnection = null;
                }

                // otherwise direct TCP
                _directConnection = new TMAPIConnection();
                _useDll = false;
                return _directConnection.Connect(_ipAddress, _port);
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect()
        {
            if (_useDll)
                _dllConnection?.Disconnect();
            else
                _directConnection?.Disconnect();
        }

        protected override byte[] ReadMemory(uint address, int length)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[TMAPI] - Not connected.");

            if (_useDll)
                return _dllConnection!.ReadMemory(address, length);

            if (length <= MAX_CHUNK_SIZE)
                return ReadChunk(address, length);

            // chunked read
            var result = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int chunkSize = Math.Min(MAX_CHUNK_SIZE, length - offset);
                byte[] chunk = ReadChunk(address + (uint)offset, chunkSize);
                Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
                offset += chunkSize;
            }
            return result;
        }

        private byte[] ReadChunk(uint address, int length)
        {
            var packet = TMAPIProtocol.BuildReadPacket(address, length, _directConnection!.NextSequence());
            var response = _directConnection.SendCommand(packet);

            if (response == null)
                throw new InvalidOperationException($"[TMAPI] - No response for read at 0x{address:X}.");

            if (response.Length < 17)
                throw new InvalidOperationException($"[TMAPI] - Response too short ({response.Length} bytes) for read at 0x{address:X}.");

            if (TMAPIProtocol.GetStatus(response) != TMAPIProtocol.STATUS_SUCCESS)
                throw new InvalidOperationException($"[TMAPI] - Read failed at 0x{address:X}, status: 0x{TMAPIProtocol.GetStatus(response):X2}.");

            return TMAPIProtocol.ExtractReadData(response);
        }

        protected override void WriteMemory(uint address, byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[TMAPI] - Not connected.");

            if (_useDll)
            {
                _dllConnection!.WriteMemory(address, data);
                return;
            }

            if (data.Length <= MAX_CHUNK_SIZE)
            {
                WriteChunk(address, data);
                return;
            }

            // chunked write
            int offset = 0;
            while (offset < data.Length)
            {
                int chunkSize = Math.Min(MAX_CHUNK_SIZE, data.Length - offset);
                byte[] chunk = new byte[chunkSize];
                Buffer.BlockCopy(data, offset, chunk, 0, chunkSize);
                WriteChunk(address + (uint)offset, chunk);
                offset += chunkSize;
            }
        }

        private void WriteChunk(uint address, byte[] data)
        {
            var packet = TMAPIProtocol.BuildWritePacket(address, data, _directConnection!.NextSequence());
            var response = _directConnection.SendCommand(packet);

            if (response == null)
                throw new InvalidOperationException($"[TMAPI] - No response for write at 0x{address:X}.");

            if (TMAPIProtocol.GetStatus(response) != TMAPIProtocol.STATUS_SUCCESS)
                throw new InvalidOperationException($"[TMAPI] - Write failed at 0x{address:X}, status: 0x{TMAPIProtocol.GetStatus(response):X2}.");
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _dllConnection?.Dispose();
                _directConnection?.Dispose();
                _disposed = true;
            }
        }

        public override string ToString() => $"[TMAPI] - {_ipAddress}:{_port} - DLL: {_useDll} - Connected: {IsConnected}";
    }
}
