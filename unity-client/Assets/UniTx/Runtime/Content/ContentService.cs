using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.ResourceManagement;

namespace UniTx.Runtime.Content
{
    public sealed class ContentService : IContentService, IContentLoader
    {
        private readonly IDictionary<string, IData> _dataRegistry = new Dictionary<string, IData>();

        public UniTask LoadContentAsync(IEnumerable<string> tags, CancellationToken cToken = default)
            => ProcessContentAsync(tags, new LoadStrategy(_dataRegistry), cToken);

        public UniTask UnloadContentAsync(IEnumerable<string> tags, CancellationToken cToken = default)
            => ProcessContentAsync(tags, new UnloadStrategy(_dataRegistry), cToken);

        public T GetData<T>(string key)
            where T : IData
        {
            if (_dataRegistry.TryGetValue(key, out var asset) && asset is T typedAsset)
            {
                return typedAsset;
            }

            throw new KeyNotFoundException($"DataAsset with Id '{key}' not found.");
        }

        public IEnumerable<T> GetData<T>(IEnumerable<string> keys)
            where T : IData
            => keys == null ? Enumerable.Empty<T>() : keys.Select(GetData<T>);

        public IEnumerable<T> GetAllData<T>()
            where T : IData
            => _dataRegistry.Values.OfType<T>();

        private async UniTask ProcessContentAsync(IEnumerable<string> tags, IProcessStrategy strategy,
            CancellationToken cToken = default)
        {
            var files = await UniResources.LoadAssetGroupAsync<TextAsset>(tags, cToken: cToken);

            foreach (var file in files)
            {
                var objs = GetDataObjects(file);

                foreach (var obj in objs)
                {
                    strategy.Process(obj);
                }
            }

            UniResources.DisposeAssetGroup(files);
        }

        private IEnumerable<IData> GetDataObjects(TextAsset file)
        {
            var fileName = file.name.FixTurkishChars();
            var loader = ContentRegistry.GetLoader(fileName);

            if (loader == null)
            {
                UniStatics.LogInfo($"File '{fileName}' not registered against any Type, skipping.", this, Color.yellow);
                return Enumerable.Empty<IData>();
            }

            var wrappedJson = $"{{ \"Items\": {file.text.FixTurkishChars()} }}";
            return loader.Load(wrappedJson);
        }
    }
}