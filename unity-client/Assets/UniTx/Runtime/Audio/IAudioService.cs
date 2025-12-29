using UnityEngine;

namespace UniTx.Runtime.Audio
{
    public interface IAudioService : IInitialisableAsync
    {
        void Play2D(IAudioConfig config);

        void Play3D(IAudioConfig config, Vector3 position);

        void PlayAttached(IAudioConfig config, Transform parent);

        void PlayMusic(IAudioConfig config);
    }
}