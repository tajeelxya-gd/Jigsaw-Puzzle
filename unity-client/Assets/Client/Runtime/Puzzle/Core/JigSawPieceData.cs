using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        public JigsawBoardCell OriginalCell { get; private set; }
        public IEnumerable<JigsawBoardCell> Cells { get; private set; }
        public PieceType PieceType { get; private set; }

        public JigSawPieceData(JigsawBoardCell originalcell, IEnumerable<JigsawBoardCell> cells, PieceType pieceType)
        {
            OriginalCell = originalcell;
            Cells = cells;
            PieceType = pieceType;
        }
    }
}
