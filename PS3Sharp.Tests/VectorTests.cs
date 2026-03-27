using PS3Sharp.Types;

namespace PS3Sharp.Tests
{
    public class VectorTests
    {
        [Fact]
        public void Vector3_Constructor()
        {
            var v = new Vector3(1.0f, 2.0f, 3.0f);
            Assert.Equal(1.0f, v.X);
            Assert.Equal(2.0f, v.Y);
            Assert.Equal(3.0f, v.Z);
        }

        [Fact]
        public void Vector3_ToString()
        {
            var v = new Vector3(1.5f, 2.5f, 3.5f);
            Assert.Equal("(1.5, 2.5, 3.5)", v.ToString());
        }

        [Fact]
        public void Vector4_Constructor()
        {
            var v = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);
            Assert.Equal(1.0f, v.X);
            Assert.Equal(2.0f, v.Y);
            Assert.Equal(3.0f, v.Z);
            Assert.Equal(4.0f, v.W);
        }

        [Fact]
        public void Vector4_ToString()
        {
            var v = new Vector4(1.5f, 2.5f, 3.5f, 4.5f);
            Assert.Equal("(1.5, 2.5, 3.5, 4.5)", v.ToString());
        }
    }
}
