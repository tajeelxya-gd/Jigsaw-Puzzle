using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UniTx.Runtime.Pool;

namespace UniTx.Runtime.Audio
{
    internal sealed class UniAudioService : IAudioService
    {
        private readonly UniSpawner _spawner = new();
        private UniAudioSource _musicSource;
        private bool _isSoundOn = true;
        private bool _isMusicOn = true;

        public bool IsSoundOn
        {
            get => _isSoundOn;
            set
            {
                if (_isSoundOn == value) return;
                _isSoundOn = value;
                _spawner.ForEachActive(item =>
                {
                    if (item is UniAudioSource source) source.Mute = !_isSoundOn;
                });
            }
        }

        public bool IsMusicOn
        {
            get => _isMusicOn;
            set
            {
                if (_isMusicOn == value) return;
                _isMusicOn = value;
                if (_musicSource != null) _musicSource.Mute = !_isMusicOn;
            }
        }

        public UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            var root = UniStatics.Root.transform;
            _musicSource = new GameObject("UniAudioSource").AddComponent<UniAudioSource>();
            _musicSource.Transform.SetParent(root);
            _spawner.SetPool(_musicSource, root, 5);
            return UniTask.CompletedTask;
        }

        public void Play2D(IAudioConfig config)
        {
            config.Data.SpatialBlend = 0f;
            var data = (UniAudioConfigData)config.Data;
            var item = _spawner.Spawn(data);
            if (item is UniAudioSource source) source.Mute = !IsSoundOn;
        }

        public void Play3D(IAudioConfig config, Vector3 position)
        {
            config.Data.SpatialBlend = 1f;
            var data = (UniAudioConfigData)config.Data;
            var item = _spawner.Spawn(data, position);
            if (item is UniAudioSource source) source.Mute = !IsSoundOn;
        }

        public void PlayAttached(IAudioConfig config, Transform parent)
        {
            config.Data.SpatialBlend = 1f;
            var data = (UniAudioConfigData)config.Data;
            data.ToFollow = parent;
            var item = _spawner.Spawn(data, parent.position, parent.rotation);
            if (item is UniAudioSource source) source.Mute = !IsSoundOn;
        }

        public void PlayMusic(IAudioConfig config)
        {
            config.Data.SpatialBlend = 0f;
            var data = (UniAudioConfigData)config.Data;
            _musicSource.SetData(data);
            _musicSource.Reset();
            _musicSource.Initialise();
            _musicSource.Mute = !IsMusicOn;
        }
    }
}