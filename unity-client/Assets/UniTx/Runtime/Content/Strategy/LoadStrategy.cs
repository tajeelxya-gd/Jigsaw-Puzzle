using System.Collections.Generic;
using UnityEngine;

namespace UniTx.Runtime.Content
{
    internal sealed class LoadStrategy : IProcessStrategy
    {
        private readonly IDictionary<string, IData> _registry;

        public LoadStrategy(IDictionary<string, IData> registry) => _registry = registry;

        public void Process(IData data)
        {
            if (_registry.TryAdd(data.Id, data)) return;

            UniStatics.LogInfo($"Duplicate Id '{data.Id}' found, conflicts with existing data.", this, Color.red);
        }
    }
}