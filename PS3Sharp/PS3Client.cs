using PS3Sharp.Backends;
using PS3Sharp.Interfaces;
using PS3Sharp.Types;

namespace PS3Sharp
{
    /// <summary>
    /// Manages the connection and interaction with a PS3 backend (RPCS3, PS3, etc.).
    /// Allows switching backend implementations at runtime.
    /// </summary>
    public class PS3Client
    {
        private IPS3API _backend;

        /// <summary>
        /// Gets the currently active backend type.
        /// </summary>
        public BackendType ActiveBackendType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PS3Client"/> class using the default RPCS3 backend.
        /// </summary>
        public PS3Client() => SelectBackend();

        /// <summary>
        /// Initializes a new instance of the <see cref="PS3Client"/> class using the PS3 backend and specified API (CCAPI/TMAPI).
        /// </summary>
        /// <param name="PS3API">The API to use for the PS3 backend.</param>
        public PS3Client(PS3Type PS3API) => SelectBackend(PS3API);

        /// <summary>
        /// Initializes a new instance of the <see cref="PS3Client"/> class using the RPCS3 backend with a custom window name.
        /// </summary>
        /// <param name="WindowName">The window name of the RPCS3 process to target.</param>
        public PS3Client(string WindowName) => Selectbackend(WindowName);

        /// <summary>
        /// Selects the default RPCS3 backend with the default window name ("rpcs3").
        /// </summary>
        public void SelectBackend() => SelectBackendInternal(BackendType.RPCS3, PS3Type.TMAPI, "rpcs3");

        /// <summary>
        /// Selects the PS3 backend with the specified API (CCAPI or TMAPI).
        /// </summary>
        /// <param name="PS3API">The API to use for PS3 communication.</param>
        public void SelectBackend(PS3Type PS3API) => SelectBackendInternal(BackendType.PS3, PS3API, "rpcs3");

        /// <summary>
        /// Selects the RPCS3 backend with a custom window name.
        /// </summary>
        /// <param name="WindowName">The window name of the RPCS3 process to attach to.</param>
        public void Selectbackend(string WindowName) => SelectBackendInternal(BackendType.RPCS3, PS3Type.TMAPI, WindowName);


        /// <summary>
        /// Selects the backend implementation to use for PS3 communication.
        /// Disconnects the current backend if connected, then switches to the new backend.
        /// </summary>
        /// <param name="BackendType">The type of backend to use.</param>
        /// <param name="PS3API">The PS3 API to use (only relevant for PS3 backend).</param>
        /// <param name="RPCS3WindowName">The window name to use for RPCS3 backend.</param>
        private void SelectBackendInternal(BackendType BackendType, PS3Type PS3API, string RPCS3WindowName)
        {
            if (_backend != null && _backend.IsConnected)
                _backend.Disconnect();

            _backend = BackendType switch
            {
                BackendType.PS3 => new PS3Backend(PS3API),
                BackendType.RPCS3 => new RPCS3Backend(RPCS3WindowName),
                _ => throw new NotImplementedException(),
            };

            ActiveBackendType = BackendType;
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

        // Memory Reads

        /// <summary>
        /// Reads a signed byte (sbyte) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The signed byte value read from memory.</returns>
        public sbyte ReadSByte(uint address) => _backend.ReadSByte(address);

        /// <summary>
        /// Reads a byte from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The byte value read from memory.</returns>
        public byte ReadByte(uint address) => _backend.ReadByte(address);

        /// <summary>
        /// Reads a byte array of specified length from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The byte array read from memory.</returns>
        public byte[] ReadBytes(uint address, int length) => _backend.ReadBytes(address, length);

        /// <summary>
        /// Reads a boolean value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The boolean value read from memory.</returns>
        public bool ReadBoolean(uint address) => _backend.ReadBoolean(address);

        /// <summary>
        /// Reads a 16-bit signed integer (short) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 16-bit signed integer read from memory.</returns>
        public short ReadInt16(uint address) => _backend.ReadInt16(address);

        /// <summary>
        /// Reads a 32-bit signed integer (int) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 32-bit signed integer read from memory.</returns>
        public int ReadInt32(uint address) => _backend.ReadInt32(address);

        /// <summary>
        /// Reads a 64-bit signed integer (long) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 64-bit signed integer read from memory.</returns>
        public long ReadInt64(uint address) => _backend.ReadInt64(address);

        /// <summary>
        /// Reads a 16-bit unsigned integer (ushort) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 16-bit unsigned integer read from memory.</returns>
        public ushort ReadUInt16(uint address) => _backend.ReadUInt16(address);

        /// <summary>
        /// Reads a 32-bit unsigned integer (uint) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 32-bit unsigned integer read from memory.</returns>
        public uint ReadUInt32(uint address) => _backend.ReadUInt32(address);

        /// <summary>
        /// Reads a 64-bit unsigned integer (ulong) from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The 64-bit unsigned integer read from memory.</returns>
        public ulong ReadUInt64(uint address) => _backend.ReadUInt64(address);

        /// <summary>
        /// Reads a 32-bit floating point value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The single-precision floating point value read from memory.</returns>
        public float ReadSingle(uint address) => _backend.ReadSingle(address);

        /// <summary>
        /// Reads a 64-bit floating point value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The double-precision floating point value read from memory.</returns>
        public double ReadDouble(uint address) => _backend.ReadDouble(address);

        /// <summary>
        /// Reads a string value from the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to read from.</param>
        /// <returns>The string read from memory.</returns>
        public string ReadString(uint address) => _backend.ReadString(address);


        // Memory Writes

        /// <summary>
        /// Writes a signed byte (sbyte) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The signed byte value to write.</param>
        public void WriteSByte(uint address, sbyte value) => _backend.WriteSByte(address, value);

        /// <summary>
        /// Writes a byte to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The byte value to write.</param>
        public void WriteByte(uint address, byte value) => _backend.WriteByte(address, value);

        /// <summary>
        /// Writes a byte array to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The byte array to write.</param>
        public void WriteBytes(uint address, byte[] value) => _backend.WriteBytes(address, value);

        /// <summary>
        /// Writes a boolean value to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The boolean value to write.</param>
        public void WriteBoolean(uint address, bool value) => _backend.WriteBoolean(address, value);

        /// <summary>
        /// Writes a 16-bit signed integer (short) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 16-bit signed integer value to write.</param>
        public void WriteInt16(uint address, short value) => _backend.WriteInt16(address, value);

        /// <summary>
        /// Writes a 32-bit signed integer (int) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 32-bit signed integer value to write.</param>
        public void WriteInt32(uint address, int value) => _backend.WriteInt32(address, value);

        /// <summary>
        /// Writes a 64-bit signed integer (long) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 64-bit signed integer value to write.</param>
        public void WriteInt64(uint address, long value) => _backend.WriteInt64(address, value);

        /// <summary>
        /// Writes a 16-bit unsigned integer (ushort) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 16-bit unsigned integer value to write.</param>
        public void WriteUInt16(uint address, ushort value) => _backend.WriteUInt16(address, value);

        /// <summary>
        /// Writes a 32-bit unsigned integer (uint) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 32-bit unsigned integer value to write.</param>
        public void WriteUInt32(uint address, uint value) => _backend.WriteUInt32(address, value);

        /// <summary>
        /// Writes a 64-bit unsigned integer (ulong) to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The 64-bit unsigned integer value to write.</param>
        public void WriteUInt64(uint address, ulong value) => _backend.WriteUInt64(address, value);

        /// <summary>
        /// Writes a 32-bit floating point value to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The single-precision floating point value to write.</param>
        public void WriteSingle(uint address, float value) => _backend.WriteSingle(address, value);

        /// <summary>
        /// Writes a 64-bit floating point value to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The double-precision floating point value to write.</param>
        public void WriteDouble(uint address, double value) => _backend.WriteDouble(address, value);

        /// <summary>
        /// Writes a string value to the specified memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The string value to write.</param>
        public void WriteString(uint address, string value) => _backend.WriteString(address, value);

        /// <summary>
        /// Calculates a final pointer address by applying a chain of offsets starting from a base address.
        /// </summary>
        /// <param name="address">The base memory address to start from.</param>
        /// <param name="offsets">An array of integer offsets to apply sequentially.</param>
        /// <returns>The computed pointer address after applying all offsets.</returns>
        public uint GetPointer(uint address, params int[] offsets) => _backend.GetPointer(address, offsets);

        #endregion
    }
}