using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PieceSnapController : MonoBehaviour
    {
        [Header("Snap Settings")]
        [SerializeField] private float snapPositionThreshold = 0.15f;
        [SerializeField] private float snapRotationThreshold = 5f;
        [SerializeField] float snapDuration = 0.2f; // duration in seconds

        private Rigidbody _rb;

        private void Awake()
        {
            TryGetComponent(out _rb);
        }

        private void OnEnable()
        {
           
        }

        private void OnDisable()
        {
           
        }

        private void HandleDragEnded(ISceneEntity entity)
        {
            if (!IsSnapCandidate())
                return;

            Snap();
        }

        private bool IsSnapCandidate()
        {
            return 0 <= snapRotationThreshold;
        }


        private async void Snap()
        {
            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            float elapsed = 0f;

            while (elapsed < snapDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / snapDuration);


                await UniTask.Yield();
            }

            // Ensure final snap is exact

            // _piece.MarkPlaced();

            UniStatics.LogInfo("Piece snapped into place", this);
        }
    }
}
