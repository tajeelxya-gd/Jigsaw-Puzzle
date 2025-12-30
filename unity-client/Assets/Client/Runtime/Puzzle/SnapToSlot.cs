using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class SnapToSlot : MonoBehaviour
    {
        [Header("Snap Settings")]
        [SerializeField] private float snapDuration = 0.2f;

        // Track slots currently overlapping
        private readonly List<Transform> _overlappingSlots = new();

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("JigsawSlot"))
            {
                if (!_overlappingSlots.Contains(other.transform))
                    _overlappingSlots.Add(other.transform);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("JigsawSlot"))
                _overlappingSlots.Remove(other.transform);
        }

        public void SnapToNearestSlot()
        {
            if (_overlappingSlots.Count == 0) return;

            // Find nearest slot
            Transform nearest = null;
            float minDist = float.MaxValue;
            foreach (var slot in _overlappingSlots)
            {
                float dist = Vector3.Distance(transform.position, slot.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = slot;
                }
            }

            if (nearest != null)
                StartCoroutine(SmoothSnap(nearest));
        }

        private IEnumerator SmoothSnap(Transform slot)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 targetPos = slot.position;
            Quaternion targetRot = slot.rotation;

            float elapsed = 0f;
            while (elapsed < snapDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / snapDuration;

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;

            // No parenting needed
        }
    }
}
