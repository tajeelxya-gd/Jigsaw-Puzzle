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

        private PuzzlePiece _piece;
        private Rigidbody _rb;

        private void Awake()
        {
            _piece = GetComponent<PuzzlePiece>();
            TryGetComponent(out _rb);
        }

        private void LateUpdate()
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
                transform.position,
                _piece.TargetPosition
            );

            if (posDistance > snapPositionThreshold)
                return false;

            var rotDistance = Quaternion.Angle(
                transform.rotation,
                _piece.TargetRotation
            );

            return rotDistance <= snapRotationThreshold;
        }

        private void Snap()
        {
            transform.SetPositionAndRotation(
                _piece.TargetPosition,
                _piece.TargetRotation
            );

            _piece.MarkPlaced();

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }

            UniStatics.LogInfo("Piece snapped into place", this);
        }
    }
}
