using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(JigsawPiece), typeof(JigsawPieceRenderer))]
    public sealed class JigsawPieceVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        private JigsawPiece _jigsawPiece;
        private JigsawPieceRenderer _renderer;

        private void Awake()
        {
            _jigsawPiece = GetComponent<JigsawPiece>();
            _renderer = GetComponent<JigsawPieceRenderer>();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            _particleSystem.Play();
            _renderer.FlashAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}