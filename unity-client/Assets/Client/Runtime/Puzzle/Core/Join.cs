using UnityEngine;

namespace Client.Runtime
{
    public sealed class Join : MonoBehaviour
    {
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Transform _mergeTransform;

        private int _neighbourIdx;

        public BoxCollider BoxCollider => _collider;

        public void Init(JigsawBoardCell cell)
        {
            if (cell == null)
            {
                _neighbourIdx = -1;
                return;
            }

            _neighbourIdx = cell.Idx;
            var cellTransform = cell.transform;
            _mergeTransform.SetPositionAndRotation(cellTransform.position, cellTransform.rotation);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<JigSawPiece>(out var piece))
            {
                if (piece.Data.OriginalIdx == _neighbourIdx)
                {
                    JoinRegistry.Register(piece, _mergeTransform);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<JigSawPiece>(out var piece))
            {
                if (piece.Data.OriginalIdx == _neighbourIdx)
                {
                    JoinRegistry.UnRegister(piece);
                }
            }
        }
    }
}