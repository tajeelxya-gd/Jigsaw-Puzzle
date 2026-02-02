using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class Toast : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _duration = 2f;
        [SerializeField] private float _fadeDuration = 0.5f;

        private CancellationTokenSource _cts;

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void Show(string message)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            ShowAsync(message, _cts.Token).Forget();
        }

        private async UniTaskVoid ShowAsync(string message, CancellationToken cToken)
        {
            _text.text = message;
            gameObject.SetActive(true);

            // Scale from 0.5 to 1.0 with Easing
            await AnimateAsync(0, 1, 0.5f, 1f, _fadeDuration, true, cToken);

            await UniTask.Delay((int)(_duration * 1000), cancellationToken: cToken);

            // Scale back down
            await AnimateAsync(1, 0, 1f, 0.8f, _fadeDuration, false, cToken);

            gameObject.SetActive(false);
        }

        private async UniTask AnimateAsync(float startAlpha, float endAlpha, float startScale, float endScale, float duration, bool useEasing, CancellationToken cToken)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Simple EaseOutCubic for entrance
                float t = useEasing ? 1f - Mathf.Pow(1f - progress, 3f) : progress;

                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                _text.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }
            _canvasGroup.alpha = endAlpha;
            _text.transform.localScale = Vector3.one * endScale;
        }
    }
}