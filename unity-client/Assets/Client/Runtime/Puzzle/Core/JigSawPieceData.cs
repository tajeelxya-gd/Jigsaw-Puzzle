using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        public JigsawBoardCell OriginalCell { get; private set; }
        public IEnumerable<JigsawBoardCell> Cells { get; private set; }

        public JigSawPieceData(JigsawBoardCell originalcell, IEnumerable<JigsawBoardCell> cells)
        {
            OriginalCell = originalcell;
            Cells = cells;
        }
    }
}
