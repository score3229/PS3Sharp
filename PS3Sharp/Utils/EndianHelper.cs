namespace PS3Sharp.Utils
{
    public static class EndianHelper
    {
        public static byte[] ToSystemEndian(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                var copy = (byte[])data.Clone();
                Array.Reverse(copy);
                return copy;
            }

            return data;
        }

        public static byte[] FromSystemEndian(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                var copy = (byte[])data.Clone();
                Array.Reverse(copy);
                return copy;
            }

            return data;
        }

        public static T Read<T>(byte[] data, Func<byte[], T> converter)
        {
            return converter(ToSystemEndian(data));
        }

        public static byte[] Write<T>(T value, Func<T, byte[]> converter)
        {
            return FromSystemEndian(converter(value));
        }
    }

}
