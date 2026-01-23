using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawPieceRenderer : MonoBehaviour, IInjectable
    {
        [Header("Reflection Settings")]
        [Range(0f, 255f)]
        [SerializeField] private float targetReflectionValue = 255f;
        [SerializeField] private float duration = 1.0f;

        private static readonly int ReflectColorId = Shader.PropertyToID("_ReflectColor");

        private IJigsawHelper _helper;
        private JigsawPieceRendererData _data;
        private MaterialPropertyBlock _mpb;
        private Renderer _shadowProxy;

        public void Inject(IResolver resolver)
        {
            _helper = resolver.Resolve<IJigsawHelper>();
        }

        public void Init(JigsawPieceRendererData data)
        {
            _data = data;
            _mpb = new MaterialPropertyBlock();
            AddShadowCaster();
            SetActive(isFlat: false);
        }

        public void SetActive(bool isFlat)
        {
            _data.Mesh.gameObject.SetActive(true);
            _data.FlatMesh.gameObject.SetActive(false);
            _shadowProxy.gameObject.SetActive(true);

            var outlineMaterial = isFlat ? _helper.SemiOutlineMaterial : _helper.OutlineMaterial;
            _data.Mesh.sharedMaterials = new[] { outlineMaterial, _helper.BaseMaterial };

            if (isFlat) LiftShadow(0.03f);
        }

        public async UniTask FlashAsync(CancellationToken token)
        {
            var renderer = _data.Mesh;
            int materialCount = renderer.sharedMaterials.Length;

            // Normalize the 0-255 input to 0-1 for Unity's Color system
            float maxIntensity = targetReflectionValue / 255f;

            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);

                    // Mathf.Sin(0 to PI) creates a perfect 0 -> 1 -> 0 curve.
                    // 0% duration = Sin(0) = 0
                    // 50% duration = Sin(PI/2) = 1
                    // 100% duration = Sin(PI) = 0
                    float t = Mathf.Sin(progress * Mathf.PI);

                    // Calculate current intensity
                    float currentVal = t * maxIntensity;
                    Color reflectColor = new Color(currentVal, currentVal, currentVal, 1f);

                    renderer.GetPropertyBlock(_mpb);
                    _mpb.SetColor(ReflectColorId, reflectColor);

                    // Apply to all materials (Outline and Base)
                    for (int i = 0; i < materialCount; i++)
                    {
                        renderer.SetPropertyBlock(_mpb, i);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            finally
            {
                // Ensure we return to 0 reflection after the duration
                _mpb.Clear();
                _mpb.SetColor(ReflectColorId, Color.black);
                for (int i = 0; i < materialCount; i++)
                {
                    renderer.SetPropertyBlock(_mpb, i);
                }
            }
        }

        private void AddShadowCaster()
        {
            var mesh = _data.Mesh;
            _shadowProxy = Instantiate(mesh, mesh.transform.parent);
            _shadowProxy.name = mesh.name + "_ShadowProxy";
            _shadowProxy.gameObject.layer = LayerMask.NameToLayer("Default");
            _shadowProxy.transform.SetPositionAndRotation(mesh.transform.position, mesh.transform.rotation);

            if (_shadowProxy.TryGetComponent<MeshRenderer>(out var r))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                r.receiveShadows = false;
            }
        }

        private void LiftShadow(float yOffset) => _shadowProxy.transform.position += Vector3.up * yOffset;
    }
}