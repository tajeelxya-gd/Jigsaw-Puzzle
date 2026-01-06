using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCompletion
    {
        private MeshRenderer _fullImageCube;
        private float _duration = 1f; // Slightly slower for a smoother "dissolve" look
        private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private CancellationTokenSource _cts;
        private Material _material;
        private static readonly int _baseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorProp = Shader.PropertyToID("_Color");

        public void SetMeshRenderer(MeshRenderer fullImageCube)
        {
            _fullImageCube = fullImageCube;

            if (_fullImageCube != null)
            {
                // Accessing .material creates a runtime instance of the embedded FBX material
                _material = _fullImageCube.material;

                // Fix rotation if FBX import is flipped
                Vector3 currentRotation = _fullImageCube.transform.localEulerAngles;
                _fullImageCube.transform.localEulerAngles = new Vector3(currentRotation.x, 180f, currentRotation.z);

                // Ensure it starts invisible
                ApplyInstantState(false);
            }
        }

        public void SetActiveFullImage(bool active, CancellationToken parentToken = default)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (!active)
            {
                ApplyInstantState(false);
                return;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            AnimateAlpha(_cts.Token).Forget();
        }

        private void ApplyInstantState(bool active)
        {
            if (_fullImageCube == null || _material == null) return;

            _fullImageCube.gameObject.SetActive(active);
            SetAlpha(active ? 1f : 0f);
        }

        private async UniTaskVoid AnimateAlpha(CancellationToken token)
        {
            if (_fullImageCube == null || _material == null) return;

            float elapsed = 0f;
            _fullImageCube.gameObject.SetActive(true);

            while (elapsed < _duration)
            {
                if (token.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _duration);
                float t = _easeCurve.Evaluate(normalizedTime);

                SetAlpha(t);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            SetAlpha(1f);
        }

        private void SetAlpha(float alpha)
        {
            int propId = _material.HasProperty(_baseColorProp) ? _baseColorProp : _colorProp;
            Color color = _material.GetColor(propId);
            color.a = alpha;
            _material.SetColor(propId, color);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}