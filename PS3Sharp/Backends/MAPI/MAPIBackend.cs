using PS3Sharp.Types;

namespace PS3Sharp.Backends.MAPI
{
    public class MAPIBackend : PS3BackendBase
    {
        private readonly MAPIConnection _connection;
        private readonly string _ipAddress;
        private readonly int _port;
        private uint _attachedPid;
        private bool _disposed;

        public override BackendType Type => BackendType.MAPI;
        public override bool IsConnected => _connection.IsConnected;

        public MAPIBackend(string ipAddress, int port = MAPIProtocol.DEFAULT_PORT)
        {
            _ipAddress = ipAddress;
            _port = port;
            _connection = new MAPIConnection();
        }

        public override bool Connect()
        {
            try
            {
                if (!_connection.Connect(_ipAddress, _port))
                    return false;

                return AttachGameProcess();
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect() => _connection.Disconnect();

        // find and attach to the first non-system process
        private bool AttachGameProcess()
        {
            var (code, message) = _connection.SendCommand(MAPIProtocol.GetAllPid());
            if (code != MAPIProtocol.OK)
                return false;

            uint[] pids = MAPIProtocol.ParsePidList(message);

            foreach (uint pid in pids)
            {
                if (pid == 0) continue;

                var (nameCode, name) = _connection.SendCommand(MAPIProtocol.GetProcessName(pid));
                if (nameCode != MAPIProtocol.OK) continue;

                // skip system processes
                if (name.Contains("dev_flash")) continue;

                _attachedPid = pid;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Manually attach to a specific process ID.
        /// </summary>
        public void AttachProcess(uint pid) => _attachedPid = pid;

        /// <summary>
        /// Gets the currently attached process ID.
        /// </summary>
        public uint AttachedProcessId => _attachedPid;

        protected override byte[] ReadMemory(uint address, int length)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[MAPI] - Not connected.");
            if (_attachedPid == 0)
                throw new InvalidOperationException("[MAPI] - No process attached.");

            string cmd = MAPIProtocol.MemoryGet(_attachedPid, address, length);
            return _connection.ReadViaPassive(cmd, _ipAddress);
        }

        protected override void WriteMemory(uint address, byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("[MAPI] - Not connected.");
            if (_attachedPid == 0)
                throw new InvalidOperationException("[MAPI] - No process attached.");

            string cmd = MAPIProtocol.MemorySet(_attachedPid, address);
            _connection.WriteViaPassive(cmd, data, _ipAddress);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _connection.Dispose();
                _disposed = true;
            }
        }

        public override string ToString() => $"[MAPI] - {_ipAddress}:{_port} - PID: 0x{_attachedPid:X} - Connected: {IsConnected}";
    }
}
