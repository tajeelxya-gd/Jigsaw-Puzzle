using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Events;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client.Runtime
{
    public sealed class JigsawBoardCompletion : IDisposable
    {
        private MeshRenderer _fullImageCube;
        private float _duration = 0.5f; // Slightly longer for a smoother "rise"
        private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Water Effect Settings")]
        [SerializeField] private float _riseDistance = 0f; // How far "underwater" it starts
        private Vector3 _targetLocalPos;

        private CancellationTokenSource _cts;
        private Material _material;
        private static readonly int _baseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorProp = Shader.PropertyToID("_Color");

        public void SetMeshRenderer(MeshRenderer fullImageCube)
        {
            _fullImageCube = fullImageCube;

            if (_fullImageCube != null)
            {
                _material = _fullImageCube.material;

                // Store the final "resting" position
                _targetLocalPos = _fullImageCube.transform.localPosition;

                // Fix rotation if FBX import is flipped
                Vector3 currentRotation = _fullImageCube.transform.localEulerAngles;
                _fullImageCube.transform.localEulerAngles = new Vector3(currentRotation.x, 180f, currentRotation.z);

                ApplyInstantState(false);
            }

            TestMethod();
        }

        public void SetActiveFullImage(bool active, CancellationToken parentToken = default)
        {
            CancelExisting();

            if (!active)
            {
                ApplyInstantState(false);
                return;
            }

            if (parentToken.IsCancellationRequested) return;

            try
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
                AnimateRiseFromWater(_cts.Token).Forget();
            }
            catch (ObjectDisposedException)
            {
                ApplyInstantState(true);
            }
        }

        private void ApplyInstantState(bool active)
        {
            if (_fullImageCube == null || _material == null) return;

            _fullImageCube.gameObject.SetActive(active);
            _fullImageCube.transform.localPosition = active ? _targetLocalPos : _targetLocalPos - Vector3.up * _riseDistance;
            SetAlpha(active ? 1f : 0f);
        }

        private async UniTaskVoid AnimateRiseFromWater(CancellationToken token)
        {
            if (_fullImageCube == null || _material == null) return;

            float elapsed = 0f;
            _fullImageCube.gameObject.SetActive(true);

            // Starting position (Below the "water" surface)
            Vector3 startPos = _targetLocalPos - Vector3.up * _riseDistance;

            try
            {
                while (elapsed < _duration)
                {
                    token.ThrowIfCancellationRequested();

                    elapsed += Time.deltaTime;
                    float normalizedTime = Mathf.Clamp01(elapsed / _duration);
                    float t = _easeCurve.Evaluate(normalizedTime);

                    // 1. Animate Position (Rising Up)
                    _fullImageCube.transform.localPosition = Vector3.Lerp(startPos, _targetLocalPos, t);

                    // 2. Animate Alpha (Fading In)
                    SetAlpha(t);

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                _fullImageCube.transform.localPosition = _targetLocalPos;
                SetAlpha(1f);
            }
            catch (OperationCanceledException) { }
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
                try
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }
                catch (ObjectDisposedException) { }
                finally { _cts = null; }
            }
        }

        public void Dispose()
        {
            CancelExisting();
            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
        }

        private void TestMethod()
        {
            UniEvents.Subscribe<DemoTestEvent>(HandleTestEvent);
            return;

            void HandleTestEvent(DemoTestEvent @event)
            {
                SetActiveFullImage(true);
            }
        }

    }
}