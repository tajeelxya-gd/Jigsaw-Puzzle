using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client.Runtime
{
    public sealed class JigsawBoardCompletion : IDisposable
    {
        private MeshRenderer _fullImageCube;
        private float _duration = 1f;
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
                // Accessing .material creates a runtime instance. 
                // We must store it to destroy it later.
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
            // 1. Clean up existing animation/token
            CancelExisting();

            if (!active)
            {
                ApplyInstantState(false);
                return;
            }

            // 2. Safety check for parent token
            if (parentToken.IsCancellationRequested) return;

            try
            {
                // 3. Create linked source and start animation
                _cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
                AnimateAlpha(_cts.Token).Forget();
            }
            catch (ObjectDisposedException)
            {
                // Handle edge case where parentToken disposes exactly as we link
                ApplyInstantState(true);
            }
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

            try
            {
                while (elapsed < _duration)
                {
                    // Check token before doing work
                    token.ThrowIfCancellationRequested();

                    elapsed += Time.deltaTime;
                    float normalizedTime = Mathf.Clamp01(elapsed / _duration);
                    float t = _easeCurve.Evaluate(normalizedTime);

                    SetAlpha(t);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                SetAlpha(1f);
            }
            catch (OperationCanceledException)
            {
                // Animation was cancelled, which is expected behavior
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_material == null) return;

            int propId = _material.HasProperty(_baseColorProp) ? _baseColorProp : _colorProp;
            Color color = _material.GetColor(propId);
            color.a = alpha;
            _material.SetColor(propId, color);
        }

        private void CancelExisting()
        {
            if (_cts != null)
            {
                // We check if it's already disposed internally or just cancel
                try
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }
                catch (ObjectDisposedException) { /* Already gone */ }
                finally
                {
                    _cts = null;
                }
            }
        }

        public void Dispose()
        {
            CancelExisting();

            // Prevent memory leak from material instance
            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
        }
    }
}