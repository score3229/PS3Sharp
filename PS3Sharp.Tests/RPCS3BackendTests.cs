namespace PS3Sharp.Tests
{
    public class RPCS3BackendTests
    {
        private PS3Client _ps3;
        private uint _testAddress;

        public RPCS3BackendTests()
        {
            _ps3 = new PS3Client("rpcs3");
            _testAddress = 0xC0000000;
        }

        [Fact]
        public void Connect_ShouldSetIsConnectedToTrue()
        {
            // act
            _ps3.Connect();

            // assert
            Assert.True(_ps3.IsConnected);
        }

        [Fact]
        public void Disconnect_ShouldSetIsConnectedToFalse()
        {
            // arrange
            _ps3.Connect();

            // act
            _ps3.Disconnect();

            // assert
            Assert.False(_ps3.IsConnected);
        }

        [Fact]
        public void WriteAndReadSByte_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            sbyte expectedValue = -45;
            _ps3.WriteSByte(_testAddress, expectedValue);

            // act
            sbyte actualValue = _ps3.ReadSByte(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadByte_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            byte expectedValue = 69;
            _ps3.WriteByte(_testAddress, expectedValue);

            // act
            byte actualValue = _ps3.ReadByte(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadBytes_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            byte[] expectedValue = new byte[] { 1, 2, 3, 4, 5 };
            _ps3.WriteBytes(_testAddress, expectedValue);

            // act
            byte[] actualValue = _ps3.ReadBytes(_testAddress, expectedValue.Length);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadBoolean_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            bool expectedValue = false;
            _ps3.WriteBoolean(_testAddress, false);

            // act
            bool actualValue = _ps3.ReadBoolean(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadInt16_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            short expectedValue = 0x1337;
            _ps3.WriteInt16(_testAddress, expectedValue);

            // act
            short actualValue = _ps3.ReadInt16(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadInt32_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            int expectedValue = 0x133337;
            _ps3.WriteInt32(_testAddress, expectedValue);

            // act
            int actualValue = _ps3.ReadInt32(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadInt64_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            long expectedValue = 0x13333337;
            _ps3.WriteInt64(_testAddress, expectedValue);

            // act
            long actualValue = _ps3.ReadInt64(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadUInt16_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            ushort expectedValue = 0x1337;
            _ps3.WriteUInt16(_testAddress, expectedValue);

            // act
            ushort actualValue = _ps3.ReadUInt16(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadUInt32_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            uint expectedValue = 0x133337;
            _ps3.WriteUInt32(_testAddress, expectedValue);

            // act
            uint actualValue = _ps3.ReadUInt32(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadUInt64_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            ulong expectedValue = 0x13333337;
            _ps3.WriteUInt64(_testAddress, expectedValue);

            // act
            ulong actualValue = _ps3.ReadUInt64(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadSingle_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            float expectedValue = 13.37f;
            _ps3.WriteSingle(_testAddress, expectedValue);

            // act
            float actualValue = _ps3.ReadSingle(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadDouble_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            double expectedValue = 1333.3337d;
            _ps3.WriteDouble(_testAddress, expectedValue);

            // act
            double actualValue = _ps3.ReadDouble(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void WriteAndReadString_ShouldReturnExpectedValue()
        {
            // arrange
            _ps3.Connect();

            // setup
            string expectedValue = "Hello World 123!";
            _ps3.WriteString(_testAddress, expectedValue);

            // act
            string actualValue = _ps3.ReadString(_testAddress);

            // assert
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
