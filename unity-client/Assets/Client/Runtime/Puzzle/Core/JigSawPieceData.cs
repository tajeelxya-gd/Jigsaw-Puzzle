using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        public int OriginalIdx { get; private set; }
        public Renderer Renderer { get; private set; }
        public IEnumerable<JigsawBoardCell> Cells { get; private set; }
        public PieceType PieceType { get; private set; }

        public JigSawPieceData(int originalIdx, Renderer renderer, IEnumerable<JigsawBoardCell> cells, PieceType pieceType)
        {
            OriginalIdx = originalIdx;
            Renderer = renderer;
            Cells = cells;
            PieceType = pieceType;
        }
    }
}
