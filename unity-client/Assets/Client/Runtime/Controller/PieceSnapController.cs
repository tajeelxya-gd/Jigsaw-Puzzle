using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float _snapDuration = 0.5f;

        private IList<JigsawBoardCell> _cells;
        private Coroutine _snapRoutine;

        public void SetCells(IList<JigsawBoardCell> cells) => _cells = cells;

        public void SnapToClosestCell()
        {
            if (_cells == null || _cells.Count == 0) return;

            Transform bestTarget = null;
            float closestDistanceSqr = float.MaxValue;
            Vector3 currentPos = transform.position;

            foreach (var cell in _cells)
            {
                if (cell == null) continue;
                var cellTransform = cell.transform;
                float distSqr = (cellTransform.position - currentPos).sqrMagnitude;
                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    bestTarget = cellTransform;
                }
            }

            if (bestTarget != null)
            {
                if (_snapRoutine != null) StopCoroutine(_snapRoutine);
                _snapRoutine = StartCoroutine(SnapLerp(bestTarget));
            }
        }

        private IEnumerator SnapLerp(Transform target)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float elapsed = 0;

            while (elapsed < _snapDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / _snapDuration;

                // Use SmoothStep for a polished "ease-in-out" feel
                float t = Mathf.SmoothStep(0, 1, normalizedTime);

                transform.position = Vector3.Lerp(startPos, target.position, t);
                transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

                yield return null;
            }

            // Finalize placement
            transform.position = target.position;
            transform.rotation = target.rotation;
            _snapRoutine = null;
        }
    }
}