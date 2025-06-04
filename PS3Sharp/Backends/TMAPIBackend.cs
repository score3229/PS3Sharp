using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS3Sharp.Backends
{
    public class TMAPIBackend
    {
        public string Name => "TMAPI";
        public bool IsConnected { get; private set; }

        public bool Connect()
        {
            // TODO: Connect using TMAPI
            IsConnected = true;
            return true;
        }

        public void Disconnect()
        {
            // TODO: disconnect from TMAPI
            IsConnected = false;
        }

        public byte[] ReadMemory(uint address, int length)
        {
            // TODO: Read memory using TMAPI
            return new byte[length];
        }

        public bool WriteMemory(uint address, byte[] data)
        {
            // TODO: Write memory using TMAPI
            return true;
        }
    }
}
