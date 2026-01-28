using System.Collections.Generic;

namespace Client.Runtime
{
    public static class JigsawBoardCalculator
    {
        public static JigsawBoard Board { get; private set; }

        public static void SetBoard(JigsawBoard board) => Board = board;

        public static NeighbourData GetNeighboursIndices(int idx)
        {
            var boardData = Board.Data;
            var rows = boardData.XConstraint;
            var cols = boardData.YConstraint;

            if (idx < 0 || idx >= (cols * rows)) return NeighbourData.None;

            int row = idx / cols;
            int col = idx % cols;

            var top = (row > 0) ? idx - cols : -1;
            var bot = (row < rows - 1) ? idx + cols : -1;
            var left = (col > 0) ? idx - 1 : -1;
            var right = (col < cols - 1) ? idx + 1 : -1;

            return new NeighbourData(top, bot, left, right);
        }

        public static IEnumerable<JigsawBoardCell> GetNeighboursCells(int idx)
        {
            var indices = GetNeighboursIndices(idx);
            var neighbours = new List<JigsawBoardCell>();
            if (indices.Top != -1) neighbours.Add(Board.Cells[indices.Top]);
            if (indices.Bottom != -1) neighbours.Add(Board.Cells[indices.Bottom]);
            if (indices.Left != -1) neighbours.Add(Board.Cells[indices.Left]);
            if (indices.Right != -1) neighbours.Add(Board.Cells[indices.Right]);
            return neighbours;
        }

        public static bool IsEdge(int idx)
        {
            var data = Board.Data;
            int r = idx / data.YConstraint;
            int c = idx % data.YConstraint;
            bool isEdge = r == 0 || r == data.XConstraint - 1 || c == 0 || c == data.YConstraint - 1;
            return isEdge;
        }
    }
}