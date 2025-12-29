using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniTx.Runtime.ResourceManagement
{
    /// <summary>
    /// Provides a unified API for loading, creating, and disposing Unity assets via a configured strategy.
    /// </summary>
    public static class UniResources
    {
        private static IResourceLoadingStrategy _strategy = null;

        internal static UniTask InitialiseAsync(IResourceLoadingStrategy strategy, CancellationToken cToken = default)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            return _strategy.InitialiseAsync(cToken);
        }

        /// <summary>
        /// Loads an asset asynchronously using the specified key.
        /// </summary>
        public static UniTask<TObject> LoadAssetAsync<TObject>(string key, IProgress<float> progress = default,
            CancellationToken cToken = default)
            where TObject : UnityEngine.Object
            => _strategy.LoadAssetAsync<TObject>(key, progress, cToken);

        /// <summary>
        /// Disposes a previously loaded asset.
        /// </summary>
        public static void DisposeAsset<TObject>(TObject asset)
            where TObject : UnityEngine.Object
            => _strategy.DisposeAsset(asset);

        /// <summary>
        /// Loads a group of assets asynchronously.
        /// </summary>
        public static UniTask<AssetGroup<TObject>> LoadAssetGroupAsync<TObject>(IEnumerable<string> labels,
            IProgress<float> progress = default, CancellationToken cToken = default)
            where TObject : UnityEngine.Object
            => _strategy.LoadAssetGroupAsync<TObject>(labels, progress, cToken);

        /// <summary>
        /// Disposes a previously loaded asset group.
        /// </summary>
        public static void DisposeAssetGroup<TObject>(AssetGroup<TObject> assetGroup)
            where TObject : UnityEngine.Object
            => _strategy.DisposeAssetGroup(assetGroup);

        /// <summary>
        /// Creates an instance of a Unity component asynchronously.
        /// </summary>
        public static UniTask<TComponent> CreateInstanceAsync<TComponent>(string key, Transform parent = null,
            IProgress<float> progress = default, CancellationToken cToken = default)
            => _strategy.CreateInstanceAsync<TComponent>(key, parent, progress, cToken);

        /// <summary>
        /// Disposes a previously created GameObject instance.
        /// </summary>
        public static bool DisposeInstance(GameObject instance) => _strategy.DisposeInstance(instance);

        /// <summary>
        /// Unloads unused Unity assets asynchronously.
        /// </summary>
        public static UniTask UnloadUnusedAssets(CancellationToken cToken = default)
            => Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: cToken);
    }
}