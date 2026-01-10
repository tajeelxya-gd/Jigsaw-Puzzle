namespace Client.Runtime
{
    public static class JigsawBoardCalculator
    {
        public static NeighbourData GetNeighbours(int idx, int cols, int rows)
        {
            if (idx < 0 || idx >= (cols * rows)) return NeighbourData.None;

            int row = idx / cols;
            int col = idx % cols;

            var top = (row > 0) ? idx - cols : -1;
            var bot = (row < rows - 1) ? idx + cols : -1;
            var left = (col > 0) ? idx - 1 : -1;
            var right = (col < cols - 1) ? idx + 1 : -1;

            return new NeighbourData(top, bot, left, right);
        }
    }
}