using System.Collections.Generic;
using UnityEngine;

namespace UniTx.Runtime.Content
{
    internal sealed class UnloadStrategy : IProcessStrategy
    {
        private readonly IDictionary<string, IData> _registry;

        public UnloadStrategy(IDictionary<string, IData> registry) => _registry = registry;

        public void Process(IData data)
        {
            if (_registry.Remove(data.Id)) return;

            UniStatics.LogInfo($"Attempted to remove unregistered Id '{data.Id}', skipping.", this, Color.red);
        }
    }
}