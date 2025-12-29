using UnityEngine;

namespace UniTx.Runtime.Audio
{
    [CreateAssetMenu(fileName = "NewAudioConfig", menuName = "UniTx/Audio/Config")]
    internal sealed class UniAudioConfig : ScriptableObject, IAudioConfig
    {
        [SerializeField] private UniAudioConfigData _data;

        public IAudioConfigData Data => _data;

        public void Play2D() => UniAudio.Play2D(this);

        public void Play3D(Vector3 position) => UniAudio.Play3D(this, position);

        public void PlayAttached(Transform parent) => UniAudio.PlayAttached(this, parent);
    }
}