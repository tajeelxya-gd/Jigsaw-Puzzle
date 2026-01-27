using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigsawPieceState
    {
        [SerializeField] private int _correctIdx = -1;
        [SerializeField] private int _currentIdx = -1;

        public JigsawPieceState(int correctIdx, int currentIdx)
        {
            _correctIdx = correctIdx;
            _currentIdx = currentIdx;
        }

        public int CorrectIdx => _correctIdx;
        public int CurrentIdx => _currentIdx;
    }
}