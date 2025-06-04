using PS3Lib;
using PS3Sharp.Interfaces;
using PS3Sharp.Types;

namespace PS3Sharp.Backends
{
    public class PS3Backend : IPS3API
    {
        private PS3API _ps3;
        public BackendType Type => BackendType.PS3;
        public bool IsConnected { get; private set; }

        public PS3Backend(SelectAPI API = SelectAPI.TargetManager)
        {
            _ps3 = new PS3API(API);
        }

        public bool Connect()
        {
            try
            {
                IsConnected = _ps3.ConnectTarget();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PS3] - Failed to connect: {ex.Message}");
                IsConnected = false;
            }

            return IsConnected;
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _ps3.DisconnectTarget();
                IsConnected = false;
            }
        }

        // basic memory access
        private byte[] ReadMemory(uint address, int length) => _ps3.GetBytes(address, length);
        private void WriteMemory(uint address, byte[] data) => _ps3.SetMemory(address, data);

        // typed memory reads
        public sbyte ReadSByte(uint address) => _ps3.Extension.ReadSByte(address);
        public byte ReadByte(uint address) => _ps3.Extension.ReadByte(address);
        public byte[] ReadBytes(uint address, int length) => _ps3.Extension.ReadBytes(address, length);
        public bool ReadBoolean(uint address) => _ps3.Extension.ReadBool(address);
        public short ReadInt16(uint address) => _ps3.Extension.ReadInt16(address);
        public int ReadInt32(uint address) => _ps3.Extension.ReadInt32(address);
        public long ReadInt64(uint address) => _ps3.Extension.ReadInt64(address);
        public ushort ReadUInt16(uint address) => _ps3.Extension.ReadUInt16(address);
        public uint ReadUInt32(uint address) => _ps3.Extension.ReadUInt32(address);
        public ulong ReadUInt64(uint address) => _ps3.Extension.ReadUInt64(address);
        public float ReadSingle(uint address) => _ps3.Extension.ReadFloat(address);
        public double ReadDouble(uint address) => _ps3.Extension.ReadDouble(address);
        public string ReadString(uint address) => _ps3.Extension.ReadString(address);

        // typed memory writes
        public void WriteSByte(uint address, sbyte value) => _ps3.Extension.WriteSByte(address, value);
        public void WriteByte(uint address, byte value) => _ps3.Extension.WriteByte(address, value);
        public void WriteBytes(uint address, byte[] value) => _ps3.Extension.WriteBytes(address, value);
        public void WriteBoolean(uint address, bool value) => _ps3.Extension.WriteBool(address, value);
        public void WriteInt16(uint address, short value) => _ps3.Extension.WriteInt16(address, value);
        public void WriteInt32(uint address, int value) => _ps3.Extension.WriteInt32(address, value);
        public void WriteInt64(uint address, long value) => _ps3.Extension.WriteInt64(address, value);
        public void WriteUInt16(uint address, ushort value) => _ps3.Extension.WriteUInt16(address, value);
        public void WriteUInt32(uint address, uint value) => _ps3.Extension.WriteUInt32(address, value);
        public void WriteUInt64(uint address, ulong value) => _ps3.Extension.WriteUInt64(address, value);
        public void WriteSingle(uint address, float value) => _ps3.Extension.WriteFloat(address, value);
        public void WriteDouble(uint address, double value) => _ps3.Extension.WriteDouble(address, value);
        public void WriteString(uint address, string value) => _ps3.Extension.WriteString(address, value);

        // pointer
        public uint GetPointer(uint address, params int[] offsets)
        {
            for (int i = 0; i < offsets.Length; i++)
            {
                address = ReadUInt32(address) + (uint)offsets[i];
            }

            return address;
        }

        // methods
        public override string ToString() => $"[PS3] - Connected: {IsConnected}";
    }
}
