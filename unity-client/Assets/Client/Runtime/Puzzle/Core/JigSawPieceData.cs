using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        private int _originalIdx;
        private Renderer _renderer;
        private IList<JigsawBoardCell> _cells;

        public int OriginalIdx => _originalIdx;
        public Renderer Renderer => _renderer;
        public IList<JigsawBoardCell> Cells => _cells;

        public JigSawPieceData(int originalIdx, Renderer renderer, IList<JigsawBoardCell> cells)
        {
            _originalIdx = originalIdx;
            _renderer = renderer;
            _cells = cells;
        }
    }
}
