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
            _data.Mesh.gameObject.AddComponent<Outline>();
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
            mesh.sharedMaterials = new[] { _helper.Base, _helper.OutlineTray, _helper.OutlineTray };

            meshTransform.localScale = new Vector3(meshTransform.localScale.x, 1f, meshTransform.localScale.z);
            mesh.GetComponent<Outline>().enabled = true;
            transform.rotation = Quaternion.Euler(_data.TrayEulers);
            SetHoverShadow();
        }

        public void OnTrayExit()
        {
            var mesh = _data.Mesh;
            var meshTransform = mesh.transform;
            mesh.sharedMaterials = new[] { _helper.Base, _helper.OutlineBoard, _helper.OutlineGrid };

            meshTransform.localScale = new Vector3(meshTransform.localScale.x, 0.2f, meshTransform.localScale.z);
            mesh.GetComponent<Outline>().enabled = false;
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public void SetIdleShadow() => SetShadowZAsync(-0.0015f, 0.1f, this.GetCancellationTokenOnDestroy()).Forget();

        public void SetHoverShadow() => SetShadowZAsync(-0.0035f, 0.1f, this.GetCancellationTokenOnDestroy()).Forget();

        public void SetShadowLayer(int layer) => _shadowProxy.gameObject.layer = layer;

        private async UniTaskVoid SetShadowZAsync(float targetZ, float duration, CancellationToken cToken = default)
        {
            Vector3 startPos = _shadowProxy.transform.localPosition;
            float startZ = startPos.z;
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    if (cToken.IsCancellationRequested) return;

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float currentZ = Mathf.Lerp(startZ, targetZ, Mathf.SmoothStep(0, 1, t));
                    _shadowProxy.transform.localPosition = new Vector3(startPos.x, startPos.y, currentZ);
                    await UniTask.Yield(PlayerLoopTiming.Update, cToken);
                }

                _shadowProxy.transform.localPosition = new Vector3(startPos.x, startPos.y, targetZ);
            }
            catch (System.OperationCanceledException)
            {
                // Empty
            }
        }

        private void AddShadowCaster()
        {
            var mesh = _data.Mesh;
            _shadowProxy = Instantiate(mesh, mesh.transform);
            _shadowProxy.name = mesh.name + "_ShadowProxy";
            SetShadowLayer(mesh.gameObject.layer);
            _shadowProxy.transform.SetLocalPositionAndRotation(new Vector3(0f, -0.001f, 0f), Quaternion.identity);

            if (_shadowProxy.TryGetComponent<MeshRenderer>(out var r))
            {
                r.receiveShadows = false;
                r.sharedMaterials = new[] { _helper.Shadow, _helper.Shadow, _helper.Shadow };
            }
        }
    }
}