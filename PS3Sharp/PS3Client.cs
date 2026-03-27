using PS3Sharp.Backends.CCAPI;
using PS3Sharp.Backends.MAPI;
using PS3Sharp.Backends.RPCS3;
using PS3Sharp.Backends.TMAPI;
using PS3Sharp.Interfaces;
using PS3Sharp.Types;

namespace PS3Sharp
{
    /// <summary>
    /// Manages the connection and interaction with a PS3 backend.
    /// Supports TMAPI (DEX), CCAPI (CEX/DEX), PS3MAPI (CEX/DEX via webMAN), and RPCS3 (emulator).
    /// </summary>
    public class PS3Client : IDisposable
    {
        private IPS3API _backend = null!;
        private bool _disposed;

        /// <summary>
        /// Gets the currently active backend type.
        /// </summary>
        public BackendType ActiveBackendType { get; private set; }

        /// <summary>
        /// Initializes a new instance targeting the RPCS3 emulator with the default process name.
        /// </summary>
        public PS3Client() => SelectBackend(BackendType.RPCS3);

        /// <summary>
        /// Initializes a new instance targeting the RPCS3 emulator with a custom process name.
        /// </summary>
        /// <param name="processName">The process name of RPCS3 to attach to.</param>
        public PS3Client(string processName) => SelectBackend(BackendType.RPCS3, processName: processName);

        /// <summary>
        /// Initializes a new instance targeting a PS3 console using the specified API and IP address.
        /// </summary>
        /// <param name="backendType">The backend type (TMAPI, CCAPI, or MAPI).</param>
        /// <param name="ipAddress">The IP address of the PS3 console.</param>
        public PS3Client(BackendType backendType, string ipAddress) => SelectBackend(backendType, ipAddress: ipAddress);

        /// <summary>
        /// Switches the active backend. Disposes the previous backend if one exists.
        /// </summary>
        public void SelectBackend(BackendType backendType, string? ipAddress = null, string? processName = null)
        {
            _backend?.Dispose();

            _backend = backendType switch
            {
                BackendType.TMAPI => new TMAPIBackend(ipAddress ?? throw new ArgumentNullException(nameof(ipAddress), "IP address is required for TMAPI.")),
                BackendType.CCAPI => new CCAPIBackend(ipAddress ?? throw new ArgumentNullException(nameof(ipAddress), "IP address is required for CCAPI.")),
                BackendType.MAPI => new MAPIBackend(ipAddress ?? throw new ArgumentNullException(nameof(ipAddress), "IP address is required for MAPI.")),
                BackendType.RPCS3 => new RPCS3Backend(processName ?? "rpcs3"),
                _ => throw new ArgumentOutOfRangeException(nameof(backendType)),
            };

            ActiveBackendType = backendType;
        }

        #region Backend Wrappers

        /// <summary>
        /// Attempts to connect to the current backend.
        /// </summary>
        /// <returns>True if connection was successful; otherwise, false.</returns>
        public bool Connect() => _backend.Connect();

        /// <summary>
        /// Disconnects from the current backend.
        /// </summary>
        public void Disconnect() => _backend.Disconnect();

        /// <summary>
        /// Gets a value indicating whether the current backend is connected.
        /// </summary>
        public bool IsConnected => _backend?.IsConnected ?? false;

        // memory reads
        public bool ReadBit(uint address, int offset) => _backend.ReadBit(address, offset);
        public sbyte ReadSByte(uint address) => _backend.ReadSByte(address);
        public byte ReadByte(uint address) => _backend.ReadByte(address);
        public byte[] ReadBytes(uint address, int length) => _backend.ReadBytes(address, length);
        public bool ReadBoolean(uint address) => _backend.ReadBoolean(address);
        public short ReadInt16(uint address) => _backend.ReadInt16(address);
        public int ReadInt32(uint address) => _backend.ReadInt32(address);
        public long ReadInt64(uint address) => _backend.ReadInt64(address);
        public ushort ReadUInt16(uint address) => _backend.ReadUInt16(address);
        public uint ReadUInt32(uint address) => _backend.ReadUInt32(address);
        public ulong ReadUInt64(uint address) => _backend.ReadUInt64(address);
        public float ReadSingle(uint address) => _backend.ReadSingle(address);
        public float ReadFloat(uint address) => ReadSingle(address);
        public double ReadDouble(uint address) => _backend.ReadDouble(address);
        public string ReadString(uint address) => _backend.ReadString(address);

        // memory writes
        public void WriteBit(uint address, int offset, bool state) => _backend.WriteBit(address, offset, state);
        public void WriteSByte(uint address, sbyte value) => _backend.WriteSByte(address, value);
        public void WriteByte(uint address, byte value) => _backend.WriteByte(address, value);
        public void WriteBytes(uint address, byte[] value) => _backend.WriteBytes(address, value);
        public void WriteBoolean(uint address, bool value) => _backend.WriteBoolean(address, value);
        public void WriteInt16(uint address, short value) => _backend.WriteInt16(address, value);
        public void WriteInt32(uint address, int value) => _backend.WriteInt32(address, value);
        public void WriteInt64(uint address, long value) => _backend.WriteInt64(address, value);
        public void WriteUInt16(uint address, ushort value) => _backend.WriteUInt16(address, value);
        public void WriteUInt32(uint address, uint value) => _backend.WriteUInt32(address, value);
        public void WriteUInt64(uint address, ulong value) => _backend.WriteUInt64(address, value);
        public void WriteSingle(uint address, float value) => _backend.WriteSingle(address, value);
        public void WriteFloat(uint address, float value) => WriteSingle(address, value);
        public void WriteDouble(uint address, double value) => _backend.WriteDouble(address, value);
        public void WriteString(uint address, string value) => _backend.WriteString(address, value);

        // enum
        public T ReadEnum<T>(uint address) where T : struct, Enum => _backend.ReadEnum<T>(address);
        public void WriteEnum<T>(uint address, T value) where T : struct, Enum => _backend.WriteEnum<T>(address, value);

        // struct
        public T ReadStruct<T>(uint address) where T : struct => _backend.ReadStruct<T>(address);
        public void WriteStruct<T>(uint address, T value) where T : struct => _backend.WriteStruct<T>(address, value);

        // vector
        public Vector3 ReadVector3(uint address) => _backend.ReadVector3(address);
        public void WriteVector3(uint address, Vector3 value) => _backend.WriteVector3(address, value);
        public Vector4 ReadVector4(uint address) => _backend.ReadVector4(address);
        public void WriteVector4(uint address, Vector4 value) => _backend.WriteVector4(address, value);

        // pointer
        public uint GetPointer(uint address, params int[] offsets) => _backend.GetPointer(address, offsets);

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _backend?.Dispose();
                _disposed = true;
            }
        }
    }
}
