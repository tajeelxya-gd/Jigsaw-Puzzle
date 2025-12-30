using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzlePiece : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform snapAnchor;

        public bool IsPlaced { get; private set; }

        public Vector3 TargetPosition => target.position;
        public Quaternion TargetRotation => target.rotation;

        public Vector3 SnapPosition =>
            snapAnchor != null ? snapAnchor.position : transform.position;

        public Quaternion SnapRotation =>
            snapAnchor != null ? snapAnchor.rotation : transform.rotation;

        public void MarkPlaced()
        {
            IsPlaced = true;
        }
    }
}
