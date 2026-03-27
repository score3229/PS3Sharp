using PS3Sharp.Interfaces;
using PS3Sharp.Types;
using PS3Sharp.Utils;
using System.Runtime.InteropServices;
using System.Text;

namespace PS3Sharp.Backends
{
    public abstract class PS3BackendBase : IPS3API
    {
        public abstract BackendType Type { get; }
        public abstract bool IsConnected { get; }
        public abstract bool Connect();
        public abstract void Disconnect();
        public abstract void Dispose();

        // subclasses implement raw memory access
        protected abstract byte[] ReadMemory(uint address, int length);
        protected abstract void WriteMemory(uint address, byte[] data);

        // typed memory reads
        public bool ReadBit(uint address, int offset)
        {
            byte value = ReadMemory(address, 1)[0];
            return (value & (1 << offset)) != 0;
        }
        public sbyte ReadSByte(uint address) => (sbyte)ReadMemory(address, 1)[0];
        public byte ReadByte(uint address) => ReadMemory(address, 1)[0];
        public byte[] ReadBytes(uint address, int length) => ReadMemory(address, length);
        public bool ReadBoolean(uint address) => BitConverter.ToBoolean(ReadMemory(address, 1), 0);
        public short ReadInt16(uint address) => EndianHelper.Read(ReadMemory(address, 2), b => BitConverter.ToInt16(b, 0));
        public int ReadInt32(uint address) => EndianHelper.Read(ReadMemory(address, 4), b => BitConverter.ToInt32(b, 0));
        public long ReadInt64(uint address) => EndianHelper.Read(ReadMemory(address, 8), b => BitConverter.ToInt64(b, 0));
        public ushort ReadUInt16(uint address) => EndianHelper.Read(ReadMemory(address, 2), b => BitConverter.ToUInt16(b, 0));
        public uint ReadUInt32(uint address) => EndianHelper.Read(ReadMemory(address, 4), b => BitConverter.ToUInt32(b, 0));
        public ulong ReadUInt64(uint address) => EndianHelper.Read(ReadMemory(address, 8), b => BitConverter.ToUInt64(b, 0));
        public float ReadSingle(uint address) => EndianHelper.Read(ReadMemory(address, 4), b => BitConverter.ToSingle(b, 0));
        public double ReadDouble(uint address) => EndianHelper.Read(ReadMemory(address, 8), b => BitConverter.ToDouble(b, 0));
        public string ReadString(uint address)
        {
            var sb = new StringBuilder();
            int offset = 0;
            const int blockSize = 40;

            while (true)
            {
                byte[] buffer = ReadMemory(address + (uint)offset, blockSize);
                int nullIndex = Array.IndexOf(buffer, (byte)0);

                if (nullIndex >= 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, nullIndex));
                    break;
                }

                sb.Append(Encoding.UTF8.GetString(buffer));
                offset += blockSize;

                if (offset > 1024)
                    throw new InvalidOperationException("String read exceeded 1024 bytes without null-termination.");
            }

            return sb.ToString();
        }

        // typed memory writes
        public void WriteBit(uint address, int offset, bool state)
        {
            byte value = ReadMemory(address, 1)[0];

            if (state)
                value |= (byte)(1 << offset);
            else
                value &= (byte)~(1 << offset);

            WriteMemory(address, new byte[] { value });
        }
        public void WriteSByte(uint address, sbyte value) => WriteMemory(address, new byte[] { (byte)value });
        public void WriteByte(uint address, byte value) => WriteMemory(address, new byte[] { value });
        public void WriteBytes(uint address, byte[] value) => WriteMemory(address, value);
        public void WriteBoolean(uint address, bool value) => WriteMemory(address, new byte[] { (byte)(value ? 1 : 0) });
        public void WriteInt16(uint address, short value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteInt32(uint address, int value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteInt64(uint address, long value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteUInt16(uint address, ushort value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteUInt32(uint address, uint value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteUInt64(uint address, ulong value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteSingle(uint address, float value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteDouble(uint address, double value) => WriteMemory(address, EndianHelper.Write(value, BitConverter.GetBytes));
        public void WriteString(uint address, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value + "\0");
            WriteMemory(address, bytes);
        }

        // enum read/write (handles underlying type with endian conversion)
        public T ReadEnum<T>(uint address) where T : struct, Enum
        {
            Type underlying = Enum.GetUnderlyingType(typeof(T));

            object val = underlying switch
            {
                _ when underlying == typeof(byte) => ReadByte(address),
                _ when underlying == typeof(sbyte) => ReadSByte(address),
                _ when underlying == typeof(short) => ReadInt16(address),
                _ when underlying == typeof(ushort) => ReadUInt16(address),
                _ when underlying == typeof(int) => ReadInt32(address),
                _ when underlying == typeof(uint) => ReadUInt32(address),
                _ when underlying == typeof(long) => ReadInt64(address),
                _ when underlying == typeof(ulong) => ReadUInt64(address),
                _ => throw new NotSupportedException($"Enum underlying type {underlying} is not supported.")
            };

            return (T)Enum.ToObject(typeof(T), val);
        }

        public void WriteEnum<T>(uint address, T value) where T : struct, Enum
        {
            Type underlying = Enum.GetUnderlyingType(typeof(T));
            object raw = Convert.ChangeType(value, underlying);

            if (underlying == typeof(byte)) WriteByte(address, (byte)raw);
            else if (underlying == typeof(sbyte)) WriteSByte(address, (sbyte)raw);
            else if (underlying == typeof(short)) WriteInt16(address, (short)raw);
            else if (underlying == typeof(ushort)) WriteUInt16(address, (ushort)raw);
            else if (underlying == typeof(int)) WriteInt32(address, (int)raw);
            else if (underlying == typeof(uint)) WriteUInt32(address, (uint)raw);
            else if (underlying == typeof(long)) WriteInt64(address, (long)raw);
            else if (underlying == typeof(ulong)) WriteUInt64(address, (ulong)raw);
            else throw new NotSupportedException($"Enum underlying type {underlying} is not supported.");
        }

        // struct read/write (auto-swaps primitive fields to big-endian for PS3)
        public T ReadStruct<T>(uint address) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] data = ReadMemory(address, size);

            SwapStructFields<T>(data);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public void WriteStruct<T>(uint address, T value) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] data = new byte[size];

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            SwapStructFields<T>(data);
            WriteMemory(address, data);
        }

        // swap multi-byte primitive fields in a struct byte array between big/little endian
        // skips single-byte types and byte arrays
        private static void SwapStructFields<T>(byte[] data) where T : struct
        {
            if (!BitConverter.IsLittleEndian)
                return;

            foreach (var field in typeof(T).GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance))
            {
                int offset = (int)Marshal.OffsetOf<T>(field.Name);
                int fieldSize = GetPrimitiveSize(field.FieldType);

                if (fieldSize > 1)
                    Array.Reverse(data, offset, fieldSize);
            }
        }

        private static int GetPrimitiveSize(Type type)
        {
            if (type == typeof(short) || type == typeof(ushort)) return 2;
            if (type == typeof(int) || type == typeof(uint)) return 4;
            if (type == typeof(long) || type == typeof(ulong)) return 8;
            if (type == typeof(float)) return 4;
            if (type == typeof(double)) return 8;
            return 0; // byte, sbyte, bool, string, arrays — no swap
        }

        // vector
        public Vector3 ReadVector3(uint address)
        {
            return new Vector3(
                ReadSingle(address),
                ReadSingle(address + 4),
                ReadSingle(address + 8));
        }

        public void WriteVector3(uint address, Vector3 value)
        {
            WriteSingle(address, value.X);
            WriteSingle(address + 4, value.Y);
            WriteSingle(address + 8, value.Z);
        }

        public Vector4 ReadVector4(uint address)
        {
            return new Vector4(
                ReadSingle(address),
                ReadSingle(address + 4),
                ReadSingle(address + 8),
                ReadSingle(address + 12));
        }

        public void WriteVector4(uint address, Vector4 value)
        {
            WriteSingle(address, value.X);
            WriteSingle(address + 4, value.Y);
            WriteSingle(address + 8, value.Z);
            WriteSingle(address + 12, value.W);
        }

        // pointer
        public uint GetPointer(uint address, params int[] offsets)
        {
            for (int i = 0; i < offsets.Length; i++)
            {
                address = ReadUInt32(address) + (uint)offsets[i];
            }

            return address;
        }
    }
}
