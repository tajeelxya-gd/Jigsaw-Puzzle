using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(JigSawPiece))]
    public sealed class JigsawPieceVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float intensity = 2f;
        [SerializeField] private float duration = 0.15f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _mpb;
        private JigSawPiece _jigsawPiece;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _jigsawPiece = GetComponent<JigSawPiece>();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            _particleSystem.Play();
            FlashAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTask FlashAsync(CancellationToken token)
        {
            var renderer = _jigsawPiece.Data.Renderer;
            renderer.GetPropertyBlock(_mpb);

            Color startEmission = Color.black;
            _mpb.SetColor(EmissionColorId, startEmission);
            renderer.SetPropertyBlock(_mpb);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI); // smooth pulse

                _mpb.SetColor(EmissionColorId, highlightColor * intensity * t);
                renderer.SetPropertyBlock(_mpb);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            // Clear emission
            _mpb.SetColor(EmissionColorId, Color.black);
            renderer.SetPropertyBlock(_mpb);
        }
    }
}