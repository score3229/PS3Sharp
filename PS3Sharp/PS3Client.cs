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

        public BackendType ActiveBackendType { get; private set; }

        public PS3Client(BackendType backendType = BackendType.RPCS3) => SelectBackend(backendType);

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