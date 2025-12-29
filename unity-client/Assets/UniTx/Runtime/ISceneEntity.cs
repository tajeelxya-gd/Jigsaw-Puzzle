using UnityEngine;

namespace UniTx.Runtime
{
    /// <summary>
    /// Represents an entity that exists within a Unity scene.
    /// </summary>
    public interface ISceneEntity
    {
        /// <summary>
        /// Gets the underlying Unity GameObject associated with this entity.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets the Transform of the associated GameObject.
        /// </summary>
        Transform Transform { get; }
    }
}