using System;
using UniTx.Runtime.Entity;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigSawBoardData : IEntityData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _name;
        [SerializeField] private int _xConstraint;
        [SerializeField] private int _yConstraint;
        [SerializeField] private string[] _piecesIds;
        [SerializeField] private string _assetDataId;
        [SerializeField] private string _fullImageId;

        public string Id => _id;
        public string Name => _name;
        public int XConstraint => _xConstraint;
        public int YConstraint => _yConstraint;
        public string[] PiecesIds => _piecesIds;
        public string AssetDataId => _assetDataId;
        public string FullImageId => _fullImageId;

        public IEntity CreateEntity() => new JigSawBoard(Id);
    }
}