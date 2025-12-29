using System.Collections.Generic;
using UnityEngine;

namespace UniTx.Runtime.Database
{
    /// <summary>
    /// Holds a collection of addressable assets identified by unique keys.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAssetData", menuName = "UniTx/AssetData")]
    public sealed class AssetData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private List<Asset> _assets = default;

        private readonly Dictionary<string, Asset> _cachedAssetsDict = new();

        /// <summary>
        /// Gets the unique identifier for this asset data set.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Gets the list of all assets defined in this data set.
        /// </summary>
        public List<Asset> Assets => _assets;

        /// <summary>
        /// Retrieves an asset by its key, caching lookups for faster access.
        /// </summary>
        public Asset GetAsset(string key)
        {
            if (_cachedAssetsDict.TryGetValue(key, out var asset))
            {
                return asset;
            }

            asset = _assets.Find(a => a.Key == key);
            _cachedAssetsDict[key] = asset ?? throw new KeyNotFoundException($"Asset with key '{key}' was not found.");
            return asset;
        }
    }

}