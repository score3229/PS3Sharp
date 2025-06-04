using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS3Sharp.Interfaces
{
    public interface IPS3API
    {
        string Name { get; }
        bool Connect();
        void Disconnect();
        bool IsConnected { get; }

        // basic memory access
        // byte[] ReadMemory(uint address, int length);
        // void WriteMemory(uint address, byte[] data);

        // typed memory reads
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

        // typed memory writes
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

        // pointer
        uint GetPointer(uint address, params int[] offsets);
    }
}