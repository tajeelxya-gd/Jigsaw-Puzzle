using System;
using UniTx.Runtime.Serialisation;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigSawBoardSavedData : ISavedData
    {
        [SerializeField] private string _id;
        [SerializeField] private long _modifiedTimestamp;

        public string Id => _id;

        public long ModifiedTimestamp { get => _modifiedTimestamp; set => _modifiedTimestamp = value; }
    }
}