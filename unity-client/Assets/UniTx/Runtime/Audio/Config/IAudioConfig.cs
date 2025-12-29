using UnityEngine;

namespace UniTx.Runtime.Audio
{
    public interface IAudioConfig
    {
        IAudioConfigData Data { get; }

        void Play2D();

        void Play3D(Vector3 position);

        void PlayAttached(Transform parent);
    }
}