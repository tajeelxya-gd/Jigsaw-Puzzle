using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPieceData
    {
        private int _originalIdx;
        private Vector3 _colliderSize;
        private IList<JigsawBoardCell> _cells;

        public int OriginalIdx => _originalIdx;
        public Vector3 ColliderSize => _colliderSize;
        public IList<JigsawBoardCell> Cells => _cells;

        public JigSawPieceData(int originalIdx, Vector3 colliderSize, IList<JigsawBoardCell> cells)
        {
            _originalIdx = originalIdx;
            _colliderSize = colliderSize;
            _cells = cells;
        }
    }
}
