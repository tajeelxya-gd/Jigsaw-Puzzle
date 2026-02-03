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
        [SerializeField] private string _imageKey;

        public string Id => _id;
        public string Name => _name;
        public string ImageKey => _imageKey;

        public IEntity CreateEntity() => new Currency(_id);
    }
}