using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigsawLevelState
    {
        [SerializeField] private string _levelId;
        [SerializeField] private JigsawPieceState[] _pieces;

        public JigsawLevelState(string levelId, JigsawPieceState[] pieces)
        {
            _levelId = levelId;
            _pieces = pieces;
        }

        public string LevelId => _levelId;
        public JigsawPieceState[] Pieces => _pieces;
    }
}