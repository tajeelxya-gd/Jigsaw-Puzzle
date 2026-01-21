using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawPieceRenderer : MonoBehaviour
    {
        [SerializeField] private Color highlightColor;
        [SerializeField] private float intensity;
        [SerializeField] private float duration;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private JigsawPieceRendererData _data;
        private MaterialPropertyBlock _mpb;
        private Renderer _shadowProxy;
        private bool _isFlat;
        private float _shadowY;

        public void Init(JigsawPieceRendererData data)
        {
            _data = data;
            _mpb = new MaterialPropertyBlock();
            AddShadowCaster();
            SetActive(isFlat: false);
        }

        public void SetActive(bool isFlat)
        {
            isFlat = false;
            _isFlat = isFlat;
            _data.Mesh.gameObject.SetActive(!isFlat);
            _data.FlatMesh.gameObject.SetActive(isFlat);
            _shadowProxy.gameObject.SetActive(!isFlat);
        }

        public async UniTask FlashAsync(CancellationToken token)
        {
            var renderer = _isFlat ? _data.FlatMesh : _data.Mesh;
            int materialCount = renderer.sharedMaterials.Length;

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

        public void LiftShadow() => SetShadowY(0.06f);

        public void UnLiftShadow() => SetShadowY(_shadowY);

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
            _shadowY = _shadowProxy.transform.position.y;
        }

        private void SetShadowY(float y)
        {
            var shadowTransform = _shadowProxy.transform;
            var position = shadowTransform.position;
            shadowTransform.position = new Vector3(position.x, y, position.z);
        }
    }
}