using PS3Sharp.Interfaces;
using PS3Sharp.Types;
using PS3Sharp.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PS3Sharp.Backends
{
    public class RPCS3Backend : IPS3API
    {
        private Process _process;
        private IntPtr _handle;
        private string _windowName;

        public BackendType Type => BackendType.RPCS3;
        public bool IsConnected { get; private set; }

        // winapi imports
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        public RPCS3Backend(string WindowName = "rpcs3") => _windowName = WindowName;

        public bool Connect()
        {
            try
            {
                _process = Process.GetProcessesByName(_windowName).FirstOrDefault();
                if (_process == null)
                    return false;

                _handle = OpenProcess(PROCESS_ALL_ACCESS, false, _process.Id);
                IsConnected = _handle != IntPtr.Zero;
            }

            catch (Exception ex)
            {
                Console.WriteLine($"[RPCS3] - Failed to connect: {ex.Message}");
                IsConnected = false;
            }

            return IsConnected;
        }

        public void Disconnect()
        {
            if (IsConnected && _handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                IsConnected = false;
            }
        }

        private byte[] ReadMemory(uint address, int length)
        {
            ulong addressEmulator;

            if (address < 0x00792000)
                addressEmulator = (ulong)address + 0x400000000;
            else
                addressEmulator = (ulong)address + 0x300000000;
            
            byte[] buffer = new byte[length];
            ReadProcessMemory(_handle, (IntPtr)addressEmulator, buffer, buffer.Length, out _);
            return buffer;
        }

        private void WriteMemory(uint address, byte[] data)
        {
            ulong addressEmulator;

            if (address < 0x00792000)
                addressEmulator = (ulong)address + 0x400000000;
            else
                addressEmulator = (ulong)address + 0x300000000;

            WriteProcessMemory(_handle, (IntPtr)addressEmulator, data, data.Length, out _);
        }

        // typed memory reads
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

                // break after reading 1024 bytes
                if (offset > 1024)
                    throw new Exception("[RPCS3] - String read exceeded 1024 bytes without null-termination.");
            }

            return sb.ToString();
        }

        // typed memory writes
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
            byte[] bytes = Encoding.ASCII.GetBytes(value + "\0");
            WriteMemory(address, bytes);
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

        // methods
        public override string ToString() => $"[RPCS3] - Connected: {IsConnected}";
    }
}
