using PS3Sharp.Backends;
using PS3Sharp.Interfaces;
using PS3Sharp.Types;

namespace PS3Sharp
{
    /// <summary>
    /// Manages the connection and interaction with a PS3 backend (RPCS3, PS3, etc.).
    /// Allows switching backend implementations at runtime.
    /// </summary>
    public class PS3Sharp
    {
        private IPS3API _backend;

        public IPS3API Backend => _backend!;
        public BackendType ActiveBackendType { get; private set; }

        public PS3Sharp(BackendType backendType = BackendType.RPCS3)
        {
            SelectBackend(backendType);
        }

        /// <summary>
        /// Selects the backend implementation to use for PS3 communication.
        /// Disconnects the current backend if connected, then switches to the new backend.
        /// </summary>
        public void SelectBackend(BackendType backendType)
        {
            if (_backend != null && _backend.IsConnected)
                _backend.Disconnect();

            _backend = backendType switch
            {
                BackendType.PS3 => new PS3Backend(),
                BackendType.RPCS3 => new RPCS3Backend(),
                _ => throw new NotImplementedException(),
            };

            ActiveBackendType = backendType;
        }

        /// <summary>
        /// Attempts to connect to the current backend.
        /// </summary>
        public bool Connect()
        {
            if (_backend == null)
                throw new InvalidOperationException("No backend set.");
            return _backend.Connect();
        }

        /// <summary>
        /// Disconnects from the current backend.
        /// </summary>
        public void Disconnect()
        {
            if (_backend == null)
                return;

            _backend.Disconnect();
        }

        /// <summary>
        /// The name of the currently active backend.
        /// </summary>
        public string ActiveBackend => _backend?.Name ?? "None";

        /// <summary>
        /// Whether the current backend is connected.
        /// </summary>
        public bool IsConnected => _backend?.IsConnected ?? false;
    }
}
