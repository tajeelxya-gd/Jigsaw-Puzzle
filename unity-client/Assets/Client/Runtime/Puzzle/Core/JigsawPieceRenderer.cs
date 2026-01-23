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
            SetActive(locked: false);
        }

        public void SetActive(bool locked)
        {
            _data.Mesh.gameObject.SetActive(true);
            _shadowProxy.gameObject.SetActive(true);

            if (locked)
            {
                // lift shadow
                SetShadowY(0.03f);
            }
        }

        public async UniTask FlashAsync(CancellationToken token)
        {
            var renderer = _data.Mesh;
            var materialCount = renderer.sharedMaterials.Length;
            var maxIntensity = targetReflectionValue / 255f;
            var elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    var progress = Mathf.Clamp01(elapsed / duration);
                    var t = Mathf.Sin(progress * Mathf.PI);
                    var currentVal = t * maxIntensity;
                    var reflectColor = new Color(currentVal, currentVal, currentVal, 1f);

                    renderer.GetPropertyBlock(_mpb);
                    _mpb.SetColor(ReflectColorId, reflectColor);

                    for (int i = 0; i < materialCount; i++)
                    {
                        renderer.SetPropertyBlock(_mpb, i);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            finally
            {
                _mpb.Clear();
                _mpb.SetColor(ReflectColorId, Color.black);
                for (int i = 0; i < materialCount; i++)
                {
                    renderer.SetPropertyBlock(_mpb, i);
                }
            }
        }

        public void SetOutlineMaterial(bool isOverTray)
        {
            var outlineMaterial = isOverTray ? _helper.PieceTrayOutline : _helper.PieceBoardOutline;
            _data.Mesh.sharedMaterials = new[] { outlineMaterial, _helper.BaseMaterial };
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

        private void SetShadowY(float yOffset) => _shadowProxy.transform.position += Vector3.up * yOffset;
    }
}