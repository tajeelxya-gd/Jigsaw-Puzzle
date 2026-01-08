using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        public JigsawBoardCell OriginalCell { get; private set; }
        public Renderer Renderer { get; private set; }
        public Renderer FlatRenderer { get; private set; }
        public IEnumerable<JigsawBoardCell> Cells { get; private set; }
        public PieceType PieceType { get; private set; }

        public JigSawPieceData(JigsawBoardCell originalcell, Renderer renderer, Renderer flatRenderer, IEnumerable<JigsawBoardCell> cells, PieceType pieceType)
        {
            OriginalCell = originalcell;
            Renderer = renderer;
            FlatRenderer = flatRenderer;
            Cells = cells;
            PieceType = pieceType;
        }
    }
}
