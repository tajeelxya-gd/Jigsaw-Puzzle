using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UniTx.Runtime.Extensions;

namespace UniTx.Runtime.ResourceManagement
{
    internal sealed class AddressablesLoadingStrategy : IResourceLoadingStrategy
    {
        private readonly Dictionary<Guid, AsyncOperationHandle> _lookup = new();

        public UniTask InitialiseAsync(CancellationToken cToken = default)
            => Addressables.InitializeAsync().ToUniTask(cToken: cToken);

        public UniTask<TObject> LoadAssetAsync<TObject>(string key, IProgress<float> progress = null,
            CancellationToken cToken = default)
            where TObject : UnityEngine.Object
        {
            return string.IsNullOrEmpty(key)
                ? UniTask.FromException<TObject>(new ArgumentException("Key cannot be null or empty."))
                : Addressables.LoadAssetAsync<TObject>(key).ToUniTask(progress, cToken: cToken);
        }

        public void DisposeAsset<TObject>(TObject asset)
            where TObject : UnityEngine.Object
            => Addressables.Release(asset);

        public async UniTask<AssetGroup<TObject>> LoadAssetGroupAsync<TObject>(IEnumerable<string> labels, IProgress<float> progress = null,
            CancellationToken cToken = default)
            where TObject : UnityEngine.Object
        {
            if (labels == null)
            {
                throw new ArgumentNullException(nameof(labels));
            }

            if (!labels.Any() || labels.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("One or more keys are null or empty.", nameof(labels));
            }

            var handle = Addressables.LoadAssetsAsync<TObject>(labels, null, Addressables.MergeMode.Union);
            var result = await handle.ToUniTask(progress, cToken: cToken);
            var assetGroup = new AssetGroup<TObject>(result);
            _lookup.Add(assetGroup.Guid, handle);

            return assetGroup;
        }

        public void DisposeAssetGroup<TObject>(AssetGroup<TObject> assetGroup)
            where TObject : UnityEngine.Object
        {
            if (_lookup.TryGetValue(assetGroup.Guid, out var handle))
            {
                _lookup.Remove(assetGroup.Guid);
                assetGroup.Dispose();
                Addressables.Release(handle);
                return;
            }

            throw new InvalidOperationException($"Trying to dispose an asset group <{assetGroup.Guid}> which is not being tracked.");
        }

        public async UniTask<TComponent> CreateInstanceAsync<TComponent>(string key, Transform parent = null, IProgress<float> progress = null,
            CancellationToken cToken = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var instance = await Addressables.InstantiateAsync(key, parent).ToUniTask(progress, cToken: cToken);

            return instance.TryGetComponent<TComponent>(out var component)
                ? component
                : throw new MissingComponentException($"{typeof(TComponent).Name} not found on {instance.name}.");
        }

        public bool DisposeInstance(GameObject instance) => Addressables.ReleaseInstance(instance);
    }
}