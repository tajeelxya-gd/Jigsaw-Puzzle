using System;
using UniTx.Runtime.Entity;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigsawBoardData : IEntityData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _name;
        [SerializeField] private int _xConstraint;
        [SerializeField] private int _yConstraint;
        [SerializeField] private string _assetDataId;
        [SerializeField] private string _gridId;
        [SerializeField] private float _trayScale;
        [SerializeField] private Vector2 _traySpacing;

        public string Id => _id;
        public string Name => _name;
        public int XConstraint => _xConstraint;
        public int YConstraint => _yConstraint;
        public string AssetDataId => _assetDataId;
        public string GridId => _gridId;
        public float TrayScale => _trayScale;
        public Vector2 TraySpacing => _traySpacing;

        public IEntity CreateEntity() => new JigsawBoard(Id);
    }
}