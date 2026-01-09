using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Client.Runtime
{
    [RequireComponent(typeof(JigsawPiece))]
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float _snapDuration = 0.25f; // Slightly faster feel for groups

        private JigsawPiece _piece;
        public event Action<JigsawBoardCell> OnSnapped;

        private void Awake() => _piece = GetComponent<JigsawPiece>();

        public void SnapToTransform(Transform target)
        {
            if (target == null) return;
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

        private async UniTask SnapAsync(JigsawBoardCell cell, CancellationToken cToken = default)
        {
            await SnapAsync(cell.transform, cToken);
            OnSnapped?.Invoke(cell);
        }

        private async UniTask SnapAsync(Transform targetTransform, CancellationToken cToken = default)
        {
            // 1. Capture the initial state of the entire group
            var group = _piece.Group;
            int count = group.Count;

            Vector3[] startPositions = new Vector3[count];
            Vector3[] relativeOffsets = new Vector3[count];
            Quaternion[] startRotations = new Quaternion[count];
            JigsawPiece[] pieces = new JigsawPiece[count];

            int i = 0;
            foreach (var p in group)
            {
                pieces[i] = p;
                startPositions[i] = p.transform.position;
                startRotations[i] = p.transform.rotation;

                // Offset of this piece relative to the piece that started the snap
                relativeOffsets[i] = p.transform.position - transform.position;
                i++;
            }

            float elapsed = 0f;

            while (elapsed < _snapDuration)
            {
                cToken.ThrowIfCancellationRequested();
                if (targetTransform == null) return;

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _snapDuration);
                float t = Mathf.SmoothStep(0f, 1f, normalizedTime);

                // 2. Move every piece in the group relative to the lead target
                for (int j = 0; j < pieces.Length; j++)
                {
                    if (pieces[j] == null) continue;

                    Vector3 targetPosWithOffset = targetTransform.position + relativeOffsets[j];
                    pieces[j].transform.position = Vector3.Lerp(startPositions[j], targetPosWithOffset, t);

                    // Usually, for jigsaw clusters, the whole group shares the same rotation alignment
                    pieces[j].transform.rotation = Quaternion.Slerp(startRotations[j], targetTransform.rotation, t);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            // 3. Finalize placement for all pieces
            if (targetTransform != null)
            {
                for (int j = 0; j < pieces.Length; j++)
                {
                    if (pieces[j] == null) continue;
                    pieces[j].transform.position = targetTransform.position + relativeOffsets[j];
                    pieces[j].transform.rotation = targetTransform.rotation;
                }
            }
        }
    }
}