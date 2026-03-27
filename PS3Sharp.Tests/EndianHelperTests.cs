using PS3Sharp.Utils;

namespace PS3Sharp.Tests
{
    public class EndianHelperTests
    {
        [Fact]
        public void Read_Int32_ConvertsFromBigEndian()
        {
            // 0x00001337 in big-endian
            byte[] be = { 0x00, 0x00, 0x13, 0x37 };
            int result = EndianHelper.Read(be, b => BitConverter.ToInt32(b, 0));
            Assert.Equal(0x1337, result);
        }

        [Fact]
        public void Write_Int32_ConvertsToBigEndian()
        {
            byte[] result = EndianHelper.Write(0x1337, BitConverter.GetBytes);
            Assert.Equal(new byte[] { 0x00, 0x00, 0x13, 0x37 }, result);
        }

        [Fact]
        public void Read_Float_ConvertsFromBigEndian()
        {
            byte[] be = EndianHelper.Write(13.37f, BitConverter.GetBytes);
            float result = EndianHelper.Read(be, b => BitConverter.ToSingle(b, 0));
            Assert.Equal(13.37f, result);
        }

        [Fact]
        public void RoundTrip_Int16()
        {
            short val = 0x7FFF;
            byte[] be = EndianHelper.Write(val, BitConverter.GetBytes);
            short result = EndianHelper.Read(be, b => BitConverter.ToInt16(b, 0));
            Assert.Equal(val, result);
        }

        [Fact]
        public void RoundTrip_Double()
        {
            double val = 1333.3337d;
            byte[] be = EndianHelper.Write(val, BitConverter.GetBytes);
            double result = EndianHelper.Read(be, b => BitConverter.ToDouble(b, 0));
            Assert.Equal(val, result);
        }
    }
}
