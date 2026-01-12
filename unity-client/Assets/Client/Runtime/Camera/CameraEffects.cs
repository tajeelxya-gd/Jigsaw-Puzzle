using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class CameraEffects : MonoBehaviour, ICameraEffects
    {
        [SerializeField] private Transform camTransform;

        [Header("Group Formed (Punch)")]
        [SerializeField] private float _punchIntensity = 0.2f;
        [SerializeField] private float _punchDuration = 0.15f;

        [ContextMenu("Play Grouping Effect")]
        public void PlayGroupingEffect() => GroupFormedAsync(this.GetCancellationTokenOnDestroy()).Forget();

        /// <summary>
        /// A "Thump" effect that feels like things snapping together.
        /// It moves the camera forward (or down) and snaps back.
        /// </summary>
        private async UniTaskVoid GroupFormedAsync(CancellationToken cToken)
        {
            Vector3 originalPos = camTransform.localPosition;
            float elapsed = 0f;

            // Phase 1: The Initial Impact (Quick Jolt)
            // We move the camera slightly on the Z-axis (zoom feel) or Y-axis (thump feel)
            Vector3 punchDir = new Vector3(0, -_punchIntensity, _punchIntensity * 0.5f);

            while (elapsed < _punchDuration)
            {
                if (cToken.IsCancellationRequested) break;

                float t = elapsed / _punchDuration;
                // Use a curve-like calculation: quickly out, smoothly back
                // Sin(PI * t) creates a perfect hump shape
                float curve = Mathf.Sin(Mathf.PI * t);

                camTransform.localPosition = originalPos + (punchDir * curve);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            camTransform.localPosition = originalPos;
        }
    }
}