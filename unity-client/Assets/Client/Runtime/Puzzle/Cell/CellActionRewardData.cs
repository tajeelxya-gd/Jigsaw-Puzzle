using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class CellActionRewardData : ICellActionData
    {
        [SerializeField] private string _id;
        [SerializeField] private int _cellIdx;
        [SerializeField] private string _rewardId;

        public string Id => _id;
        public int CellIdx => _cellIdx;
        public string RewardId => _rewardId;
    }
}