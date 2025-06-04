using PS3Sharp.Interfaces;
using PS3Sharp.Types;

namespace PS3Sharp.Tests
{
    public class RPCS3BackendTests
    {
        private PS3Sharp _ps3;
        private IPS3API _backend;
        private uint _testAddress;

        public RPCS3BackendTests()
        {
            _ps3 = new PS3Sharp(BackendType.RPCS3);
            _backend = _ps3.Backend;
            _testAddress = 0xC0000000;
        }

        [Fact]
        public void Connect_ShouldSetIsConnectedToTrue()
        {
            // act
            _backend.Connect();

            // assert
            Assert.True(_backend.IsConnected);
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
        public void WriteAndReadSByte_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            sbyte expectedValue = -45;
            _backend.WriteSByte(_testAddress, expectedValue);

            // act
            sbyte actualValue = _backend.ReadSByte(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadByte_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            byte expectedValue = 69;
            _backend.WriteByte(_testAddress, expectedValue);

            // act
            byte actualValue = _backend.ReadByte(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadBytes_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            byte[] expectedValue = new byte[] { 1, 2, 3, 4, 5 };
            _backend.WriteBytes(_testAddress, expectedValue);

            // act
            byte[] actualValue = _backend.ReadBytes(_testAddress, expectedValue.Length);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadBoolean_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            bool expectedValue = false;
            _backend.WriteBoolean(_testAddress, false);

            // act
            bool actualValue = _backend.ReadBoolean(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadInt16_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            short expectedValue = 0x1337;
            _backend.WriteInt16(_testAddress, expectedValue);

            // act
            short actualValue = _backend.ReadInt16(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadInt32_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            int expectedValue = 0x133337;
            _backend.WriteInt32(_testAddress, expectedValue);

            // act
            int actualValue = _backend.ReadInt32(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadInt64_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            long expectedValue = 0x13333337;
            _backend.WriteInt64(_testAddress, expectedValue);

            // act
            long actualValue = _backend.ReadInt64(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadUInt16_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            ushort expectedValue = 0x1337;
            _backend.WriteUInt16(_testAddress, expectedValue);

            // act
            ushort actualValue = _backend.ReadUInt16(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadUInt32_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            uint expectedValue = 0x133337;
            _backend.WriteUInt32(_testAddress, expectedValue);

            // act
            uint actualValue = _backend.ReadUInt32(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadUInt64_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            ulong expectedValue = 0x13333337;
            _backend.WriteUInt64(_testAddress, expectedValue);

            // act
            ulong actualValue = _backend.ReadUInt64(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadSingle_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            float expectedValue = 13.37f;
            _backend.WriteSingle(_testAddress, expectedValue);

            // act
            float actualValue = _backend.ReadSingle(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadDouble_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            double expectedValue = 1333.3337d;
            _backend.WriteDouble(_testAddress, expectedValue);

            // act
            double actualValue = _backend.ReadDouble(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadString_ShouldReturnExpectedValue()
        {
            // arrange
            _backend.Connect();

            // setup
            string expectedValue = "Hello World 123!";
            _backend.WriteString(_testAddress, expectedValue);

            // act
            string actualValue = _backend.ReadString(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
