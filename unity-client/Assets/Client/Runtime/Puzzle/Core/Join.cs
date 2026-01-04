using UnityEngine;

namespace Client.Runtime
{
    public sealed class Join : MonoBehaviour
    {
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Transform _mergeTransform;

        private bool _isReady;
        private int _neighbourIdx;
        private JigSawPiece _piece;

        public BoxCollider BoxCollider => _collider;

        public void Init(JigsawBoardCell cell, JigSawPiece piece)
        {
            _piece = piece;

            if (cell == null)
            {
                _isReady = false;
                _neighbourIdx = -1;
                return;
            }

            _neighbourIdx = cell.Idx;
            var cellTransform = cell.transform;
            _mergeTransform.SetPositionAndRotation(cellTransform.position, cellTransform.rotation);
            _isReady = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isReady || _piece == null) return;

            if (collision.gameObject.TryGetComponent<JigSawPiece>(out var piece))
            {
                if (piece.Data.OriginalIdx == _neighbourIdx)
                {
                    piece.transform.SetPositionAndRotation(_mergeTransform.position, _mergeTransform.rotation);
                    piece.Group.Add(_piece);
                    _piece.Group.Add(piece);
                    _isReady = false;
                }
            }
        }
    }
}