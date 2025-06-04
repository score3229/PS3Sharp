using PS3Sharp.Types;

namespace PS3Sharp.Tests
{
    public class PS3BackendTests
    {
        private PS3Client _ccapi;
        private PS3Client _tampi;
        private uint _testAddress;

        public PS3BackendTests()
        {
            _tampi = new PS3Client(PS3Type.TMAPI);
            _ccapi = new PS3Client(PS3Type.CCAPI);
            _testAddress = 0xC0000000;
        }

        [Fact]
        public void InitialTest()
        {
            // TODO: Implement memory read/write tests for PS3 (TMAPI/CCAPI)
        }
    }
}