using System;
using UniTx.Runtime.Content;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigSawLevelData : IData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _boardId;
        [SerializeField] private string _imageKey;

        public string Id => _id;
        public string BoardId => _boardId;
        public string ImageKey => _imageKey;
    }
}