using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class ScaleController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _threshold = 0.0001f;

        private CancellationTokenSource _scaleCts;

        public void ScaleTo(float targetValue) => StartScaleAnimation(targetValue);

        public void SetScaleInstant(float targetValue)
        {
            CancelCurrent();
            transform.localScale = Vector3.one * targetValue;
        }

        private void StartScaleAnimation(float target)
        {
            CancelCurrent();
            _scaleCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ScaleAsync(target, _scaleCts.Token).Forget();
        }

        private void CancelCurrent()
        {
            if (_scaleCts != null)
            {
                _scaleCts.Cancel();
                _scaleCts.Dispose();
                _scaleCts = null;
            }
        }

        private async UniTaskVoid ScaleAsync(float targetValue, CancellationToken cToken)
        {
            Vector3 targetScale = Vector3.one * targetValue;

            try
            {
                while ((transform.localScale - targetScale).sqrMagnitude > _threshold)
                {
                    float smoothing = 1.0f - Mathf.Exp(-_speed * Time.deltaTime);

                    transform.localScale = Vector3.Lerp(
                        transform.localScale,
                        targetScale,
                        smoothing
                    );

                    await UniTask.Yield(PlayerLoopTiming.Update, cToken);
                }

                transform.localScale = targetScale;
            }
            catch (System.OperationCanceledException)
            {
                // Empty
            }
        }

        private void OnDestroy() => CancelCurrent();
    }
}