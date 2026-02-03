using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public class CurrencyRewardData : IRewardData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _currencyId;
        [SerializeField] private int _amountMin;
        [SerializeField] private int _amountMax;

        public string Id => _id;
        public string CurrencyId => _currencyId;
        public int AmountMin => _amountMin;
        public int AmountMax => _amountMax;
    }
}