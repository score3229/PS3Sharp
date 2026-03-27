using PS3Sharp.Backends.TMAPI;

namespace PS3Sharp.Tests
{
    public class ProtocolTests
    {
        [Fact]
        public void TMAPIProtocol_Checksum()
        {
            // known good: the write packet from our Wireshark capture
            // payload bytes after header, checksum should be 0xe2
            byte[] payload = {
                0x48, 0x54, 0x00, 0x10, 0x00, 0x00, 0x02, 0x00,
                0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x23, 0xd5,
                0x00, 0x00, 0x00, 0x0c, 0x01, 0x01, 0x02, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xb0, 0x00, 0x00,
                0xde, 0xad, 0xbe, 0xef
            };
            Assert.Equal(0xe2, TMAPIProtocol.CalculateChecksum(payload));
        }

        [Fact]
        public void TMAPIProtocol_BuildInitPacket_Length()
        {
            var packet = TMAPIProtocol.BuildInitPacket();
            Assert.Equal(52, packet.Length);
            // magic
            Assert.Equal(0x30, packet[0]);
            Assert.Equal(0x10, packet[1]);
            // type MT
            Assert.Equal(0x4d, packet[8]);
            Assert.Equal(0x54, packet[9]);
        }

        [Fact]
        public void TMAPIProtocol_BuildReadPacket_ContainsAddress()
        {
            var packet = TMAPIProtocol.BuildReadPacket(0x00B00000, 4, 0x1000);
            // address at offset 36-39 in the packet
            Assert.Equal(0x00, packet[36]);
            Assert.Equal(0xB0, packet[37]);
            Assert.Equal(0x00, packet[38]);
            Assert.Equal(0x00, packet[39]);
        }

        [Fact]
        public void TMAPIProtocol_BuildWritePacket_ContainsData()
        {
            byte[] data = { 0xDE, 0xAD, 0xBE, 0xEF };
            var packet = TMAPIProtocol.BuildWritePacket(0x00B00000, data, 0x1000);
            // data at offset 40-43
            Assert.Equal(0xDE, packet[40]);
            Assert.Equal(0xAD, packet[41]);
            Assert.Equal(0xBE, packet[42]);
            Assert.Equal(0xEF, packet[43]);
        }
    }
}
