using System.Collections.Generic;

namespace Client.Runtime
{
    public static class JigsawBoardCalculator
    {
        private static JigSawBoard _board;

        public static void SetBoard(JigSawBoard board) => _board = board;

        public static IEnumerable<JigsawBoardCell> GetNeighbours(int idx)
        {
            var data = _board.Data;
            var cols = data.YConstraint;
            var rows = data.XConstraint;
            var cells = _board.Cells;

            var neighbours = new JigsawBoardCell[4];

            if (idx < 0 || idx >= cells.Count) return neighbours;

            int row = idx / cols;
            int col = idx % cols;

            // Top
            neighbours[0] = (row > 0) ? cells[idx - cols] : null;

            // Bottom
            neighbours[1] = (row < rows - 1) ? cells[idx + cols] : null;

            // Left
            neighbours[2] = (col > 0) ? cells[idx - 1] : null;

            // Right
            neighbours[3] = (col < cols - 1) ? cells[idx + 1] : null;

            return neighbours;
        }

    }
}