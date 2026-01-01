using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UniTx.Runtime.Extensions;

namespace Client.Runtime
{
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float _snapDuration = 0.5f;

        public event Action<int> OnSnapped;

        public void SnapToClosestCell(IList<JigsawBoardCell> cells)
        {
            if (cells == null || cells.Count == 0) return;

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

        private async UniTask SnapAsync(JigsawBoardCell cell, CancellationToken cToken = default)
        {
            var startPos = transform.position;
            var startRot = transform.rotation;
            var cellTransform = cell.transform;

            float elapsed = 0f;

            while (elapsed < _snapDuration)
            {
                cToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / _snapDuration;

                // Ease-in-out
                float t = Mathf.SmoothStep(0f, 1f, normalizedTime);

                transform.position = Vector3.Lerp(startPos, cellTransform.position, t);
                transform.rotation = Quaternion.Slerp(startRot, cellTransform.rotation, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            // Finalize placement
            transform.position = cellTransform.position;
            transform.rotation = cellTransform.rotation;

            OnSnapped.Broadcast(cell.Idx);
        }
    }
}
