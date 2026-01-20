using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Extensions;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float _snapDuration = 0.25f;

        public event Action<JigsawBoardCell, float> OnSnapped;

        public void SnapToClosestCell(JigsawGroup group, IEnumerable<JigsawBoardCell> cells)
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
            SnapToCellAsync(group, bestTarget, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public async UniTask SnapToCellAsync(JigsawGroup group, JigsawBoardCell cell, CancellationToken cToken = default)
        {
            var cellTransform = cell.transform;
            float targetHeight = cell.GetNextHeight(group.Count);
            var snapPos = new Vector3(cellTransform.position.x, targetHeight, cellTransform.position.z);
            await SnapAsync(group, snapPos, cellTransform.rotation, cToken);
            OnSnapped?.Invoke(cell, targetHeight);
        }

        private async UniTask SnapAsync(JigsawGroup group, Vector3 snapPos, Quaternion snapRot, CancellationToken cToken = default)
        {
            // 1. Capture the initial state of the entire group
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

                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / _snapDuration);
                float t = Mathf.SmoothStep(0f, 1f, normalizedTime);

                // 2. Move every piece in the group relative to the lead target
                for (int j = 0; j < pieces.Length; j++)
                {
                    if (pieces[j] == null) continue;

                    Vector3 targetPosWithOffset = snapPos + relativeOffsets[j];
                    pieces[j].transform.position = Vector3.Lerp(startPositions[j], targetPosWithOffset, t);

                    // Usually, for jigsaw clusters, the whole group shares the same rotation alignment
                    pieces[j].transform.rotation = Quaternion.Slerp(startRotations[j], snapRot, t);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            // 3. Finalize placement for all pieces
            for (int j = 0; j < pieces.Length; j++)
            {
                if (pieces[j] == null) continue;
                pieces[j].transform.position = snapPos + relativeOffsets[j];
                pieces[j].transform.rotation = snapRot;
            }
        }
    }
}