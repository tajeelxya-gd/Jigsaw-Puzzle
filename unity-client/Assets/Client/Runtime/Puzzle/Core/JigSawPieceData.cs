using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        public int OriginalIdx { get; private set; }
        public Renderer Renderer { get; private set; }
        public IEnumerable<JigsawBoardCell> Cells { get; private set; }
        public IEnumerable<int> NeighboursIndices { get; private set; }

        public JigSawPieceData(int originalIdx, Renderer renderer, IEnumerable<JigsawBoardCell> cells, IEnumerable<int> neighboursIndices)
        {
            OriginalIdx = originalIdx;
            Renderer = renderer;
            Cells = cells;
            NeighboursIndices = neighboursIndices;
        }
    }
}
