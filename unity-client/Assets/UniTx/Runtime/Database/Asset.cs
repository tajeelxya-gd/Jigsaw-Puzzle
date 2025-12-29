using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniTx.Runtime.Database
{
    /// <summary>
    /// Represents an addressable asset with an associated key and reference.
    /// </summary>
    [Serializable]
    public sealed class Asset
    {
        [SerializeField] private string _key = default;
        [SerializeField] private AssetReference _reference = default;

        /// <summary>
        /// Gets the unique key associated with this asset.
        /// </summary>
        public string Key => _key;

        /// <summary>
        /// Gets the runtime key assigned by the Addressables system.
        /// </summary>
        public string RuntimeKey => _reference.RuntimeKey.ToString();
    }
}