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
            _spawner.Spawn(data);
        }

        public void Play3D(IAudioConfig config, Vector3 position)
        {
            config.Data.SpatialBlend = 1f;
            var data = (UniAudioConfigData)config.Data;
            _spawner.Spawn(data, position);
        }

        public void PlayAttached(IAudioConfig config, Transform parent)
        {
            config.Data.SpatialBlend = 1f;
            var data = (UniAudioConfigData)config.Data;
            data.ToFollow = parent;
            _spawner.Spawn(data, parent.position, parent.rotation);
        }

        public void PlayMusic(IAudioConfig config)
        {
            config.Data.SpatialBlend = 0f;
            var data = (UniAudioConfigData)config.Data;
            _musicSource.SetData(data);
            _musicSource.Reset();
            _musicSource.Initialise();
        }
    }
}