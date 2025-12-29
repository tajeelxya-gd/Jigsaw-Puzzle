using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace UniTx.Runtime.Audio
{
    public static class UniAudio
    {
        private static IAudioService _audioService = null;

        internal static UniTask InitialiseAsync(IAudioService audioService, CancellationToken cToken = default)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            return _audioService.InitialiseAsync(cToken);
        }

        public static void Play2D(IAudioConfig config)
            => _audioService.Play2D(config);

        public static void Play3D(IAudioConfig config, Vector3 position)
            => _audioService.Play3D(config, position);

        public static void PlayAttached(IAudioConfig config, Transform parent)
            => _audioService.PlayAttached(config, parent);

        public static void PlayMusic(IAudioConfig config)
            => _audioService.PlayMusic(config);
    }
}