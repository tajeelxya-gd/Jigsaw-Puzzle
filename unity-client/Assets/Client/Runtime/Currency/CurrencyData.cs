using System;
using UniTx.Runtime.Entity;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public class CurrencyData : ICurrencyData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _name;

        public string Id => _id;
        public string Name => _name;

        public IEntity CreateEntity() => new Currency(_id);
    }
}