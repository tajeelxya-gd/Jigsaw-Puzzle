using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniTx.Runtime.ResourceManagement
{
    /// <summary>
    /// Defines a strategy for loading, creating, and disposing Unity assets and instances.
    /// </summary>
    public interface IResourceLoadingStrategy : IInitialisableAsync
    {
        /// <summary>
        /// Loads an asset asynchronously using the specified key.
        /// </summary>
        UniTask<TObject> LoadAssetAsync<TObject>(string key, IProgress<float> progress = default,
            CancellationToken cToken = default)
            where TObject : UnityEngine.Object;

        /// <summary>
        /// Disposes a previously loaded asset.
        /// </summary>
        void DisposeAsset<TObject>(TObject asset)
            where TObject : UnityEngine.Object;

        /// <summary>
        /// Loads a group of assets asynchronously using the provided labels.
        /// </summary>
        UniTask<AssetGroup<TObject>> LoadAssetGroupAsync<TObject>(IEnumerable<string> labels, IProgress<float> progress = default,
            CancellationToken cToken = default)
            where TObject : UnityEngine.Object;

        /// <summary>
        /// Disposes a previously loaded asset group.
        /// </summary>
        void DisposeAssetGroup<TObject>(AssetGroup<TObject> assetGroup)
            where TObject : UnityEngine.Object;

        /// <summary>
        /// Creates an instance of a component asynchronously using the specified key.
        /// </summary>
        UniTask<TComponent> CreateInstanceAsync<TComponent>(string key, Transform parent = null, IProgress<float> progress = default,
            CancellationToken cToken = default);

        /// <summary>
        /// Disposes a previously created GameObject instance.
        /// </summary>
        bool DisposeInstance(GameObject instance);
    }
}