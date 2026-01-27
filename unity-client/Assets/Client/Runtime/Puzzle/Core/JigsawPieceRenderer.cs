using System;
using System.Threading;
using cakeslice;
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

        private IJigsawResourceLoader _helper;
        private JigsawPieceRendererData _data;
        private MaterialPropertyBlock _mpb;
        private Renderer _shadowProxy;

        public void Inject(IResolver resolver)
        {
            _helper = resolver.Resolve<IJigsawResourceLoader>();
        }

        public void Init(JigsawPieceRendererData data)
        {
            _data = data;
            _mpb = new MaterialPropertyBlock();
            AddShadowCaster();
            SetActive(locked: false);
            _data.Mesh.gameObject.AddComponent<Outline>();
        }

        public void SetActive(bool locked)
        {
            _data.Mesh.gameObject.SetActive(true);
            _shadowProxy.gameObject.SetActive(true);

            if (locked)
            {
                // lift shadow
                SetShadowY(0.02f);
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

        public void OnTrayEnter()
        {
            var mesh = _data.Mesh;
            var meshTransform = mesh.transform;
            mesh.sharedMaterials = new[] { _helper.OutlineTray, _helper.Base };

            meshTransform.localScale = new Vector3(meshTransform.localScale.x, 1f, meshTransform.localScale.z);
            mesh.GetComponent<Outline>().enabled = true;
            transform.rotation = Quaternion.Euler(_data.TrayEulers);
            SetShadowY(0);
        }

        public void OnTrayExit()
        {
            var mesh = _data.Mesh;
            var meshTransform = mesh.transform;
            mesh.sharedMaterials = new[] { _helper.OutlineBoard, _helper.Base };

            meshTransform.localScale = new Vector3(meshTransform.localScale.x, 0.2f, meshTransform.localScale.z);
            mesh.GetComponent<Outline>().enabled = false;
            transform.rotation = Quaternion.Euler(Vector3.zero);
            SetShadowY(-0.03f);
        }

        private void AddShadowCaster()
        {
            var mesh = _data.Mesh;
            _shadowProxy = Instantiate(mesh, mesh.transform);
            _shadowProxy.name = mesh.name + "_ShadowProxy";
            _shadowProxy.gameObject.layer = LayerMask.NameToLayer("Default");
            _shadowProxy.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (_shadowProxy.TryGetComponent<MeshRenderer>(out var r))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                r.receiveShadows = false;
            }
        }

        private void SetShadowY(float yOffset) => _shadowProxy.transform.localPosition = Vector3.up * yOffset;
    }
}