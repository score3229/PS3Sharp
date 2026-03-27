using PS3Sharp.Types;

namespace PS3Sharp.Interfaces
{
    public interface IPS3API : IDisposable
    {
        BackendType Type { get; }

        bool Connect();
        void Disconnect();
        bool IsConnected { get; }

        // memory reads
        bool ReadBit(uint address, int offset);
        sbyte ReadSByte(uint address);
        byte ReadByte(uint address);
        byte[] ReadBytes(uint address, int length);
        bool ReadBoolean(uint address);
        short ReadInt16(uint address);
        int ReadInt32(uint address);
        long ReadInt64(uint address);
        ushort ReadUInt16(uint address);
        uint ReadUInt32(uint address);
        ulong ReadUInt64(uint address);
        float ReadSingle(uint address);
        double ReadDouble(uint address);
        string ReadString(uint address);

        // memory writes
        void WriteBit(uint address, int offset, bool state);
        void WriteSByte(uint address, sbyte value);
        void WriteByte(uint address, byte value);
        void WriteBytes(uint address, byte[] value);
        void WriteBoolean(uint address, bool value);
        void WriteInt16(uint address, short value);
        void WriteInt32(uint address, int value);
        void WriteInt64(uint address, long value);
        void WriteUInt16(uint address, ushort value);
        void WriteUInt32(uint address, uint value);
        void WriteUInt64(uint address, ulong value);
        void WriteSingle(uint address, float value);
        void WriteDouble(uint address, double value);
        void WriteString(uint address, string value);

        // enum
        T ReadEnum<T>(uint address) where T : struct, Enum;
        void WriteEnum<T>(uint address, T value) where T : struct, Enum;

        // struct
        T ReadStruct<T>(uint address) where T : struct;
        void WriteStruct<T>(uint address, T value) where T : struct;

        // vector
        Vector3 ReadVector3(uint address);
        void WriteVector3(uint address, Vector3 value);
        Vector4 ReadVector4(uint address);
        void WriteVector4(uint address, Vector4 value);

        // pointer
        uint GetPointer(uint address, params int[] offsets);
    }
}