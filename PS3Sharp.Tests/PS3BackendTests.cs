using PS3Sharp.Backends;
using PS3Sharp.Interfaces;
using PS3Sharp.Types;

namespace PS3Sharp.Tests
{
    public class PS3BackendTests
    {
        private PS3Sharp _ps3;
        private IPS3API _backend;
        private uint _testAddress;

        public PS3BackendTests()
        {
            _ps3 = new PS3Sharp(BackendType.PS3);
            _backend = _ps3.Backend;
            _testAddress = 0xC0000000;
        }

        [Fact]
        public void Test1()
        {

        }
    }
}