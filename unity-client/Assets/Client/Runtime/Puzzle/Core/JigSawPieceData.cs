using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigsawPieceData
    {
        public JigsawBoardCell OriginalCell { get; private set; }
        public IEnumerable<JigsawBoardCell> Cells { get; private set; }

        public JigsawPieceData(JigsawBoardCell originalcell, IEnumerable<JigsawBoardCell> cells)
        {
            OriginalCell = originalcell;
            Cells = cells;
        }
    }
}
