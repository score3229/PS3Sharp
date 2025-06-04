using PS3Sharp.Backends;
using Xunit;

namespace PS3Sharp.Tests
{
    public class RPCS3BackendTests
    {
        private RPCS3Backend _backend;
        private uint _testAddress;

        public RPCS3BackendTests()
        {
            _backend = new RPCS3Backend();
            _testAddress = 0xC0000000;
        }

        [Fact]
        public void Disconnect_ShouldSetIsConnectedToFalse()
        {
            // arrange
            _backend.Connect();

            // act
            _backend.Disconnect();

            // assert
            Assert.False(_backend.IsConnected);
        }

        [Fact]
        public void WriteAndReadInt32_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            int expectedValue = 0x1337;
            _backend.WriteInt32(_testAddress, expectedValue);

            // act
            int actualValue = _backend.ReadInt32(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
