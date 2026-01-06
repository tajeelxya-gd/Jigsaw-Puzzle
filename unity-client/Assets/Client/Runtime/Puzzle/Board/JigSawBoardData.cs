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
        [SerializeField] private string _assetDataId;
        [SerializeField] private string _fullImageId;
        [SerializeField] private string _gridId;
        [SerializeField] private float _trayScaleReduction;

        public string Id => _id;
        public string Name => _name;
        public int XConstraint => _xConstraint;
        public int YConstraint => _yConstraint;
        public string AssetDataId => _assetDataId;
        public string FullImageId => _fullImageId;
        public string GridId => _gridId;
        public float TrayScaleReduction => _trayScaleReduction;

        public IEntity CreateEntity() => new JigSawBoard(Id);
    }
}