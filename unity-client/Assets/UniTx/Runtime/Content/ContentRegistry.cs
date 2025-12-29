using System;
using System.Collections.Generic;

namespace UniTx.Runtime.Content
{
    public static class ContentRegistry
    {
        private static readonly Dictionary<string, IDataLoader> _loaders = new();

        public static void Register<T>(string fileName)
            where T : class, IData
        {
            var type = typeof(T);

            if (type.IsInterface || type.IsAbstract)
            {
                throw new InvalidOperationException($"Cannot register an interface or abstract type: {type.Name}.");
            }

            _loaders[fileName] = new DataLoader<T>();
        }

        internal static IDataLoader GetLoader(string fileName)
            => _loaders.TryGetValue(fileName, out var loader) ? loader : null;
    }
}