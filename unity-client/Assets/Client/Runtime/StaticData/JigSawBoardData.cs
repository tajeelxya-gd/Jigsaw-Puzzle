using System;
using UniTx.Runtime.Content;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigSawBoardData : IData
    {
        [SerializeField] private string _id;
        [SerializeField] private int _xConstraint;
        [SerializeField] private int _yConstraint;
        [SerializeField] private string[] _piecesIds;
        [SerializeField] private string _assetDataId;

        public string Id => _id;
        public int XConstraint => _xConstraint;
        public int YConstraint => _yConstraint;
        public string[] PiecesIds => _piecesIds;
        public string AssetDataId => _assetDataId;
    }
}