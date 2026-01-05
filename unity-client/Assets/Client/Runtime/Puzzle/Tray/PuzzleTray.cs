using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleTray : MonoBehaviour, IPuzzleTray
    {
        [Header("Grid Settings")]
        [SerializeField] private int _rowCount = 2;
        [SerializeField] private Vector2 _spacing = new Vector2(0.06f, 0.05f);
        [SerializeField] private float _scaleReduction = 0.5f;
        [SerializeField] private Vector2 _padding = new Vector2(0.02f, 0.02f);
        [SerializeField] private BoxCollider _trayCollider;

        [Header("Smoothness Settings")]
        [SerializeField] private float _lerpSpeed = 5f;

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSpeed = 0.1f;
        [SerializeField] private float _visibilityBuffer = 0.01f;
        [SerializeField] private float _dragThreshold = 10f;

        private readonly List<JigSawPiece> _activePieces = new();
        private JigSawPiece _hitPiece;
        private float _scrollX = 0f;
        private Vector3 _startMousePos;
        private Vector3 _lastMousePos;
        private bool _isDragging;
        private bool _scrollLocked;

        public void ShufflePieces(IReadOnlyList<JigSawPiece> pieces)
        {
            if (pieces == null || pieces.Count == 0) return;

            _activePieces.Clear();
            _activePieces.AddRange(pieces);

            for (int i = 0; i < _activePieces.Count; i++)
            {
                int randomIndex = Random.Range(i, _activePieces.Count);
                (_activePieces[i], _activePieces[randomIndex]) = (_activePieces[randomIndex], _activePieces[i]);
                _activePieces[i].gameObject.SetActive(true);
            }

            _scrollX = 0;
        }

        private void Update()
        {
            HandleScrollInput();
            UpdatePiecePositions();
        }

        private void HandleScrollInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == _trayCollider)
                {
                    _isDragging = true;
                    _scrollLocked = false;
                    _startMousePos = Input.mousePosition;
                    _lastMousePos = Input.mousePosition;
                    _hitPiece = GetPieceAtPosition(hit.point);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                _scrollLocked = false;
                _hitPiece = null;
            }

            if (_isDragging)
            {
                Vector3 currentMousePos = Input.mousePosition;

                if (!_scrollLocked)
                {
                    float diffX = Mathf.Abs(currentMousePos.x - _startMousePos.x);
                    float diffY = Mathf.Abs(currentMousePos.y - _startMousePos.y);

                    if (diffX > _dragThreshold && diffX > diffY)
                    {
                        _scrollLocked = true;
                    }
                    else if (diffY > _dragThreshold && diffY > diffX)
                    {
                        if (_hitPiece != null)
                        {
                            PickUpPiece(_hitPiece);
                        }
                        _isDragging = false;
                    }
                }

                if (_scrollLocked)
                {
                    float deltaX = (currentMousePos.x - _lastMousePos.x) / Screen.width;
                    _scrollX += deltaX * _scrollSpeed;

                    int cols = Mathf.CeilToInt((float)_activePieces.Count / _rowCount);
                    float totalWidth = (cols - 1) * _spacing.x;
                    float trayWidth = _trayCollider.size.x;
                    float maxScroll = Mathf.Max(0, totalWidth - trayWidth + (_padding.x * 2));

                    _scrollX = Mathf.Clamp(_scrollX, -maxScroll, 0);
                }

                _lastMousePos = currentMousePos;
            }
        }

        private JigSawPiece GetPieceAtPosition(Vector3 worldPoint)
        {
            if (_activePieces.Count == 0) return null;

            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            float localStartX = _trayCollider.center.x - (_trayCollider.size.x / 2f) + _padding.x + _scrollX;
            float localStartZ = _trayCollider.center.z + (_trayCollider.size.z / 2f) - _padding.y;

            int col = Mathf.RoundToInt((localPoint.x - localStartX) / _spacing.x);
            int row = Mathf.RoundToInt((localStartZ - localPoint.z) / _spacing.y);

            int cols = Mathf.CeilToInt((float)_activePieces.Count / _rowCount);
            int index = (row * cols) + col;

            if (index >= 0 && index < _activePieces.Count)
            {
                if (_activePieces[index].gameObject.activeSelf)
                    return _activePieces[index];
            }
            return null;
        }

        private void PickUpPiece(JigSawPiece piece)
        {
            // IMPORTANT: Remove from list first so UpdatePiecePositions stops controlling it
            _activePieces.Remove(piece);

            piece.transform.SetParent(null);
            piece.transform.localScale = Vector3.one; // Reset scale instantly

            piece.StartManualDrag();
        }

        private void UpdatePiecePositions()
        {
            if (_activePieces.Count == 0 || _trayCollider == null) return;

            Vector3 localTopLeft = new Vector3(
                _trayCollider.center.x - (_trayCollider.size.x / 2f) + _padding.x + _scrollX,
                _trayCollider.center.y,
                _trayCollider.center.z + (_trayCollider.size.z / 2f) - _padding.y
            );

            int cols = Mathf.CeilToInt((float)_activePieces.Count / _rowCount);

            for (int i = 0; i < _activePieces.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;

                Transform pt = _activePieces[i].transform;
                if (pt.parent != transform) pt.SetParent(transform);

                Vector3 targetPos = new Vector3(
                    localTopLeft.x + (col * _spacing.x),
                    localTopLeft.y,
                    localTopLeft.z - (row * _spacing.y)
                );

                // Apply Lerp to position
                pt.localPosition = Vector3.Lerp(pt.localPosition, targetPos, Time.deltaTime * _lerpSpeed);

                // Apply Lerp to scale while in tray
                Vector3 targetScale = Vector3.one * _scaleReduction;
                pt.localScale = Vector3.Lerp(pt.localScale, targetScale, Time.deltaTime * _lerpSpeed);

                pt.localRotation = Quaternion.identity;

                float localX = pt.localPosition.x;
                float leftEdge = _trayCollider.center.x - (_trayCollider.size.x / 2f) - _visibilityBuffer;
                float rightEdge = _trayCollider.center.x + (_trayCollider.size.x / 2f) + _visibilityBuffer;

                _activePieces[i].gameObject.SetActive(localX >= leftEdge && localX <= rightEdge);
            }
        }
    }
}