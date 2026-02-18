using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public class CellActionPuzzleManiaData : ICellActionData
    {
        [SerializeField] private string _id;
        [SerializeField] private int _cellIdx;
        [SerializeField] private int _rewardAmount;

        public string Id => _id;
        public int CellIdx => _cellIdx;
        public int RewardAmount => _rewardAmount;
    }
}