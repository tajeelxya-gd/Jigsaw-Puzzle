namespace Client.Runtime
{
    public readonly struct NeighbourData
    {
        public readonly int Top;
        public readonly int Bottom;
        public readonly int Left;
        public readonly int Right;

        public NeighbourData(int top, int bottom, int left, int right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

        public static NeighbourData None = new(-1, -1, -1, -1);
    }
}