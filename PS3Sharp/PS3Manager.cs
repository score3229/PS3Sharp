using PS3Sharp.Interfaces;

namespace PS3Sharp
{
    public class PS3Manager
    {
        private IPS3API _backend;

        public PS3Manager(IPS3API backend)
        {
            _backend = backend;
        }

        public void SetBackend(IPS3API backend)
        {
            if (_backend != null && _backend.IsConnected)
                _backend.Disconnect();

            _backend = backend;
        }

        public bool Connect() => _backend?.Connect() ?? false;
        public void Disconnect() => _backend?.Disconnect();
        public byte[]? ReadMemory(uint address, int length) => _backend?.ReadMemory(address, length);
        public bool WriteMemory(uint address, byte[] data) => _backend?.WriteMemory(address, data) ?? false;

        public string ActiveBackend => _backend?.Name ?? "None";
        public bool IsConnected => _backend?.IsConnected ?? false;
    }
}
