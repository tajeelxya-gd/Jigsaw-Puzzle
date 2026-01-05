using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace Client.Runtime
{
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float _snapDuration = 0.5f;

        public event Action<JigsawBoardCell> OnSnapped;

        /// <summary>
        /// Snaps the piece to a specific transform without a cell reference.
        /// </summary>
        public void SnapToTransform(Transform target)
        {
            if (target == null) return;

            // Start the async snap process for a generic transform
            SnapAsync(target, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void SnapToClosestCell(IEnumerable<JigsawBoardCell> cells)
        {
            if (cells == null) return;

            JigsawBoardCell bestTarget = null;
            float closestDistanceSqr = float.MaxValue;
            Vector3 currentPos = transform.position;

            foreach (var cell in cells)
            {
                if (cell == null) continue;

                float distSqr = (cell.transform.position - currentPos).sqrMagnitude;
                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    bestTarget = cell;
                }
            }

            if (bestTarget == null) return;

            SnapAsync(bestTarget, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Handles snapping to a JigsawBoardCell and triggers the OnSnapped event.
        /// </summary>
        private async UniTask SnapAsync(JigsawBoardCell cell, CancellationToken cToken = default)
        {
            await SnapAsync(cell.transform, cToken);

            // Trigger event safely
            OnSnapped?.Invoke(cell);
        }

        /// <summary>
        /// Core snapping logic for any transform target.
        /// </summary>
        private async UniTask SnapAsync(Transform targetTransform, CancellationToken cToken = default)
        {
            var startPos = transform.position;
            var startRot = transform.rotation;

            float elapsed = 0f;

            while (elapsed < _snapDuration)
            {
                // Ensure we handle cancellation (e.g., object destroyed during move)
                cToken.ThrowIfCancellationRequested();

                // Safety check in case the target is destroyed during the animation
                if (targetTransform == null) return;

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _snapDuration);

                // Ease-in-out curve
                float t = Mathf.SmoothStep(0f, 1f, normalizedTime);

                transform.position = Vector3.Lerp(startPos, targetTransform.position, t);
                transform.rotation = Quaternion.Slerp(startRot, targetTransform.rotation, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            // Finalize placement
            if (targetTransform != null)
            {
                transform.position = targetTransform.position;
                transform.rotation = targetTransform.rotation;
            }
        }
    }
}