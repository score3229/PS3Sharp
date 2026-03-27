namespace PS3Sharp.Types
{
    public struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
