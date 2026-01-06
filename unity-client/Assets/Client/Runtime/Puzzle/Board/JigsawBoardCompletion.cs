using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCompletion
    {
        private MeshRenderer _fullImageCube;
        private float _duration = 0.5f;
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
                // 1. Instantiate the material for transparency control
                _material = _fullImageCube.material;

                // 2. Set Y rotation to 180 degrees // TODO: remove this after correct fpx reimport
                Vector3 currentRotation = _fullImageCube.transform.localEulerAngles;
                _fullImageCube.transform.localEulerAngles = new Vector3(currentRotation.x, 180f, currentRotation.z);
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
            AnimateCube(true, _cts.Token).Forget();
        }

        private void ApplyInstantState(bool active)
        {
            if (_fullImageCube == null) return;

            _fullImageCube.transform.localScale = active ? Vector3.one : Vector3.zero;

            if (_material != null)
            {
                Color color = _material.HasProperty(_baseColorProp)
                    ? _material.GetColor(_baseColorProp)
                    : _material.GetColor(_colorProp);

                color.a = active ? 1f : 0f;

                if (_material.HasProperty(_baseColorProp))
                    _material.SetColor(_baseColorProp, color);
                else
                    _material.SetColor(_colorProp, color);
            }

            _fullImageCube.gameObject.SetActive(active);
        }

        private async UniTaskVoid AnimateCube(bool active, CancellationToken token)
        {
            if (_fullImageCube == null || _material == null) return;

            float elapsed = 0f;
            Vector3 startScale = _fullImageCube.transform.localScale;
            Vector3 targetScale = Vector3.one;

            float startAlpha = 0f;
            float targetAlpha = 1f;

            _fullImageCube.gameObject.SetActive(true);

            while (elapsed < _duration)
            {
                if (token.IsCancellationRequested || _fullImageCube == null) return;

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _duration);
                float t = _easeCurve.Evaluate(normalizedTime);

                _fullImageCube.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                Color color = _material.HasProperty(_baseColorProp)
                    ? _material.GetColor(_baseColorProp)
                    : _material.GetColor(_colorProp);

                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);

                if (_material.HasProperty(_baseColorProp))
                    _material.SetColor(_baseColorProp, color);
                else
                    _material.SetColor(_colorProp, color);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (_fullImageCube != null)
            {
                _fullImageCube.transform.localScale = targetScale;
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}