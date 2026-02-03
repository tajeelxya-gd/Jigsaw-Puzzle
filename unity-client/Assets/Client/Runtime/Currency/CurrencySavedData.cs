using System;
using UniTx.Runtime.Serialisation;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public class CurrencySavedData : ISavedData
    {
        [SerializeField] private string _id;
        [SerializeField] private long _modifiedTimestamp;
        [SerializeField] private double _amount;

        public string Id => _id;
        public long ModifiedTimestamp { get => _modifiedTimestamp; set => _modifiedTimestamp = value; }
        public double Amount => _amount;

        public void SetAmount(double amount) => _amount = amount;
    }
}