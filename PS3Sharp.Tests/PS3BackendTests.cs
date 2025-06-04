using PS3Sharp.Types;

namespace PS3Sharp.Tests
{
    public class PS3BackendTests
    {
        private PS3Client _ps3;
        private uint _testAddress;

        public PS3BackendTests()
        {
            _ps3 = new PS3Client(BackendType.PS3);
            _testAddress = 0xC0000000;
        }

        [Fact]
        public void Test1()
        {

        }
    }
}