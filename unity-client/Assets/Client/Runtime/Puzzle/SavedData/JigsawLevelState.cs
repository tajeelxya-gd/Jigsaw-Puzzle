using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigsawLevelState
    {
        [SerializeField] private JigsawPieceState[] _pieces;

        public JigsawLevelState(JigsawPieceState[] pieces)
        {
            _pieces = pieces;
        }

        public JigsawPieceState[] Pieces => _pieces;
    }
}