using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawPieceRenderer : MonoBehaviour, IInjectable
    {
        [SerializeField] private Color highlightColor;
        [SerializeField] private float intensity;
        [SerializeField] private float duration;

        private static readonly int MainColorId = Shader.PropertyToID("_Color");
        private static readonly int ReflectColorId = Shader.PropertyToID("_ReflectColor");

        private IJigsawHelper _helper;
        private JigsawPieceRendererData _data;
        private MaterialPropertyBlock _mpb;
        private Renderer _shadowProxy;
        private bool _isFlat;

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
            _isFlat = isFlat;
            _data.Mesh.gameObject.SetActive(true);
            _data.FlatMesh.gameObject.SetActive(false);
            _shadowProxy.gameObject.SetActive(true);
            var outlineMaterial = isFlat ? _helper.SemiOutlineMaterial : _helper.OutlineMaterial;
            _data.Mesh.sharedMaterials = new[] { outlineMaterial, _helper.BaseMaterial };
            // _data.FlatMesh.sharedMaterials = new[] { _helper.BaseMaterial };
        }

        public async UniTask FlashAsync(CancellationToken token)
        {
            var renderer = _data.Mesh;
            int materialCount = renderer.sharedMaterials.Length;

            // 1. Capture original values from the shared materials before starting
            // We look at the first material as the reference for the "original" state
            var baseMat = renderer.sharedMaterial;
            Color originalColor = baseMat.HasProperty(MainColorId) ? baseMat.GetColor(MainColorId) : Color.white;
            Color originalReflect = baseMat.HasProperty(ReflectColorId) ? baseMat.GetColor(ReflectColorId) : new Color(0.5f, 0.5f, 0.5f, 0.5f);

            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float normalizedTime = Mathf.Clamp01(elapsed / duration);
                    float t = Mathf.Sin(normalizedTime * Mathf.PI);

                    // 2. Lerp from the CAPTURED original to the highlight
                    Color lerpedColor = Color.Lerp(originalColor, highlightColor * intensity, t);
                    Color lerpedReflect = Color.Lerp(originalReflect, highlightColor * intensity, t);

                    renderer.GetPropertyBlock(_mpb);
                    _mpb.SetColor(MainColorId, lerpedColor);
                    _mpb.SetColor(ReflectColorId, lerpedReflect);

                    for (int i = 0; i < materialCount; i++)
                    {
                        renderer.SetPropertyBlock(_mpb, i);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            finally
            {
                // 3. Guaranteed Reset: Clear the PropertyBlock overrides 
                // This makes the renderer revert to the exact values in the sharedMaterials
                renderer.GetPropertyBlock(_mpb);
                _mpb.Clear();
                for (int i = 0; i < materialCount; i++)
                {
                    renderer.SetPropertyBlock(_mpb, i);
                }
            }
        }

        private void AddShadowCaster()
        {
            var mesh = _data.Mesh;
            var meshTransform = mesh.transform;
            _shadowProxy = GameObject.Instantiate(mesh, meshTransform.parent);
            _shadowProxy.name = mesh.name + "_ShadowProxy";
            _shadowProxy.gameObject.layer = LayerMask.NameToLayer("Default");
            _shadowProxy.transform.SetPositionAndRotation(meshTransform.position, meshTransform.rotation);
            if (_shadowProxy.TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                renderer.receiveShadows = false;
            }
        }
    }
}