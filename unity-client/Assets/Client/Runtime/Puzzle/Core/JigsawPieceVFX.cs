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
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _renderer = GetComponent<JigsawPieceRenderer>();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            UniTask.Void(PlayAsync, _cts.Token);
        }

        private async UniTaskVoid PlayAsync(CancellationToken cToken = default)
        {
            try
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _particleSystem.Play();

                await _renderer.FlashAsync(cToken);
            }
            catch (System.OperationCanceledException)
            {
                // Silently catch the cancellation when a new Play() is called
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}