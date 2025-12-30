using UnityEngine;

namespace Client.Runtime
{
    public class PuzzlePiece : MonoBehaviour
    {
        public bool IsPlaced { get; private set; }

        [SerializeField] private Transform target;

        public Vector3 TargetPosition => target.position;
        public Quaternion TargetRotation => target.rotation;

        public void MarkPlaced()
        {
            IsPlaced = true;
        }
    }
}
