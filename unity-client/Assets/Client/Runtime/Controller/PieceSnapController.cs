using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(PuzzlePiece))]
    public sealed class PieceSnapController : MonoBehaviour
    {
        [Header("Snap Settings")]
        [SerializeField] private float snapPositionThreshold = 0.15f;
        [SerializeField] private float snapRotationThreshold = 5f;
        [SerializeField] float snapDuration = 0.2f; // duration in seconds

        private PuzzlePiece _piece;
        private Rigidbody _rb;

        private void Awake()
        {
            _piece = GetComponent<PuzzlePiece>();
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
            if (_piece.IsPlaced)
                return;

            if (!IsSnapCandidate())
                return;

            Snap();
        }

        private bool IsSnapCandidate()
        {
            var posDistance = Vector3.Distance(
                _piece.SnapPosition,
                _piece.TargetPosition
            );

            if (posDistance > snapPositionThreshold)
                return false;

            var rotDistance = Quaternion.Angle(
                _piece.SnapRotation,
                _piece.TargetRotation
            );

            return rotDistance <= snapRotationThreshold;
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
            Vector3 endPos = _piece.TargetPosition;
            Quaternion endRot = _piece.TargetRotation;

            float elapsed = 0f;

            while (elapsed < snapDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / snapDuration);

                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);

                await UniTask.Yield();
            }

            // Ensure final snap is exact
            transform.SetPositionAndRotation(endPos, endRot);

            // _piece.MarkPlaced();

            UniStatics.LogInfo("Piece snapped into place", this);
        }
    }
}
