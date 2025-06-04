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

        byte[] ReadMemory(uint address, int length);
        bool WriteMemory(uint address, byte[] data);

        bool IsConnected { get; }
    }
}