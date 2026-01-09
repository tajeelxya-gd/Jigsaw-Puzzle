using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawPieceRenderer : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float intensity;
        [SerializeField] private float duration;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private JigsawPieceRendererData _data;
        private MaterialPropertyBlock _mpb;
        private bool _isFlat;

        public void Init(JigsawPieceRendererData data)
        {
            _data = data;
            _mpb = new MaterialPropertyBlock();
            SetTexture(_data.Mesh, _data.Texture);
            SetTexture(_data.FlatMesh, _data.Texture);
            SetActive(isFlat: false);
        }

        public void SetActive(bool isFlat)
        {
            _isFlat = isFlat;
            _data.Mesh.gameObject.SetActive(!isFlat);
            _data.FlatMesh.gameObject.SetActive(isFlat);
        }

        public async UniTask FlashAsync(CancellationToken token)
        {
            var renderer = _isFlat ? _data.FlatMesh : _data.Mesh;
            int materialCount = renderer.sharedMaterials.Length;

            Color startEmission = Color.black;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                Color currentColor = highlightColor * intensity * t;

                renderer.GetPropertyBlock(_mpb);
                _mpb.SetColor(EmissionColorId, currentColor);

                for (int i = 0; i < materialCount; i++)
                {
                    renderer.SetPropertyBlock(_mpb, i);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _mpb.SetColor(EmissionColorId, Color.black);
            for (int i = 0; i < materialCount; i++)
            {
                renderer.SetPropertyBlock(_mpb, i);
            }
        }

        private void SetTexture(Renderer renderer, Texture2D texture)
        {
            renderer.material.EnableKeyword("_EMISSION");
            var sharedMaterials = renderer.materials;

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                var sharedMaterial = sharedMaterials[i];
                if (sharedMaterial != null)
                {
                    sharedMaterial.SetTexture("_BaseMap", texture);
                }
            }

            renderer.materials = sharedMaterials;
        }
    }
}