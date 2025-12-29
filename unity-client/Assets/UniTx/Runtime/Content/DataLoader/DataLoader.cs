using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniTx.Runtime.Content
{
    internal class DataLoader<TData> : IDataLoader
        where TData : IData
    {
        [Serializable]
        private class Wrapper
        {
            public TData[] Items;
        }

        public IEnumerable<IData> Load(string json)
        {
            var wrapper = JsonUtility.FromJson<Wrapper>(json);
            return wrapper?.Items.Cast<IData>() ?? Array.Empty<IData>();
        }
    }
}