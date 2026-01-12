using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(JigsawPieceRenderer))]
    public sealed class JigsawPieceVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        private JigsawPieceRenderer _renderer;
        private bool _isPlaying;

        private void Awake()
        {
            _renderer = GetComponent<JigsawPieceRenderer>();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            if (_isPlaying) return;
            UniTask.Void(PlayAsync, this.GetCancellationTokenOnDestroy());
        }

        private async UniTaskVoid PlayAsync(CancellationToken cToken = default)
        {
            _isPlaying = true;
            _particleSystem.Play();
            await _renderer.FlashAsync(cToken);
            _isPlaying = false;
        }
    }
}