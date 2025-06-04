using PS3Sharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS3Sharp.Backends
{
    public class CCAPIBackend : IPS3API
    {
        public string Name => "CCAPI";
        public bool IsConnected { get; private set; }

        public bool Connect()
        {
            // TODO: Use PS3Lib or custom code to connect via CCAPI
            IsConnected = true;
            return true;
        }

        public void Disconnect()
        {
            // TODO: disconnect from CCAPI
            IsConnected = false;
        }

        public byte[] ReadMemory(uint address, int length)
        {
            // TODO: Read memory using CCAPI
            return new byte[length];
        }

        public bool WriteMemory(uint address, byte[] data)
        {
            // TODO: Write memory using CCAPI
            return true;
        }
    }
}
