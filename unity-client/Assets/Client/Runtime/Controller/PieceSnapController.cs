using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float _snapDuration = 0.5f;

        private IList<Transform> _placements;
        private Coroutine _snapRoutine;

        public void SetPlacements(IList<Transform> placements) => _placements = placements;

        public void SnapToClosestPlacement()
        {
            if (_placements == null || _placements.Count == 0) return;

            Transform bestTarget = null;
            float closestDistanceSqr = float.MaxValue;
            Vector3 currentPos = transform.position;

            // Find the closest placement regardless of distance
            foreach (var placement in _placements)
            {
                if (placement == null) continue;

                float distSqr = (placement.position - currentPos).sqrMagnitude;
                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    bestTarget = placement;
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