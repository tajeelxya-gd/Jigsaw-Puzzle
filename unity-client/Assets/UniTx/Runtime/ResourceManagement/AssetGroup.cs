using System;
using System.Collections.Generic;

namespace UniTx.Runtime.ResourceManagement
{
    /// <summary>
    /// Represents a disposable group of loaded Unity assets.
    /// </summary>
    public sealed class AssetGroup<TObject> : List<TObject>, IDisposable
        where TObject : UnityEngine.Object
    {
        /// <summary>
        /// Unique identifier for this asset group.
        /// </summary>
        public readonly Guid Guid;

        /// <summary>
        /// Creates a new asset group using the provided collection.
        /// </summary>
        internal AssetGroup(IEnumerable<TObject> collection) : base(collection) => Guid = Guid.NewGuid();

        /// <summary>
        /// Disposes the group by clearing all contained assets.
        /// </summary>
        public void Dispose() => Clear();
    }
}
