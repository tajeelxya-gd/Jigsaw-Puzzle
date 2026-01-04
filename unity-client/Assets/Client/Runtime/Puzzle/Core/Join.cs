using UnityEngine;

namespace Client.Runtime
{
    public sealed class Join : MonoBehaviour
    {
        [SerializeField] private BoxCollider _collider;

        private bool _isReady;
        private int _neighbourIdx;

        public BoxCollider BoxCollider => _collider;

        public void Init(JigsawBoardCell cell)
        {
            if (cell == null)
            {
                _isReady = false;
                return;
            }

            _neighbourIdx = cell.Idx;
            var cellTransform = cell.transform;
            transform.SetPositionAndRotation(cellTransform.position, cellTransform.rotation);
            _isReady = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isReady) return;

            if (collision.gameObject.TryGetComponent<JigSawPiece>(out var piece))
            {
                if (piece.Data.OriginalIdx == _neighbourIdx)
                {
                    piece.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    _isReady = false;
                }
            }
        }
    }
}