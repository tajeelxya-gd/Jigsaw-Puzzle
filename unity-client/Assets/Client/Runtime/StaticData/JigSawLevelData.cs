using System;
using UniTx.Runtime.Content;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigsawLevelData : IData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _name;
        [SerializeField] private string _gridId;
        [SerializeField] private string _imageKey;
        [SerializeField] private int _difficulty;
        [SerializeField] private string[] _cellActionIds;

        public string Id => _id;
        public string Name => _name;
        public string GridId => _gridId;
        public string ImageKey => _imageKey;
        public int Difficulty => _difficulty;
        public string[] CellActionIds => _cellActionIds;
    }
}