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

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSpeed = 0.1f; // Adjusted for screen percentage
        [SerializeField] private float _visibilityBuffer = 0.01f;
        [SerializeField] private float _dragThreshold = 10f; // Pixels move before locking drag

        private readonly List<JigSawPiece> _activePieces = new();
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
            UpdatePiecePositions();
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
                if (Physics.Raycast(ray, out RaycastHit hit) &&
                   (hit.collider == _trayCollider || hit.transform.IsChildOf(transform)))
                {
                    _isDragging = true;
                    _scrollLocked = false;
                    _startMousePos = Input.mousePosition;
                    _lastMousePos = Input.mousePosition;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                _scrollLocked = false;
            }

            if (_isDragging)
            {
                Vector3 currentMousePos = Input.mousePosition;

                // 1. Check if we should lock into scrolling mode
                if (!_scrollLocked)
                {
                    float diffX = Mathf.Abs(currentMousePos.x - _startMousePos.x);
                    float diffY = Mathf.Abs(currentMousePos.y - _startMousePos.y);

                    // Only lock if horizontal movement is significantly greater than vertical
                    if (diffX > _dragThreshold && diffX > diffY)
                    {
                        _scrollLocked = true;
                    }
                    // If vertical is dominant, we might want to cancel the scroll drag
                    else if (diffY > _dragThreshold)
                    {
                        _isDragging = false;
                    }
                }

                // 2. Perform scrolling if locked
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

                pt.localPosition = new Vector3(
                    localTopLeft.x + (col * _spacing.x),
                    localTopLeft.y,
                    localTopLeft.z - (row * _spacing.y)
                );

                pt.localRotation = Quaternion.identity;
                pt.localScale = Vector3.one * _scaleReduction;

                float localX = pt.localPosition.x;
                float leftEdge = _trayCollider.center.x - (_trayCollider.size.x / 2f) - _visibilityBuffer;
                float rightEdge = _trayCollider.center.x + (_trayCollider.size.x / 2f) + _visibilityBuffer;

                _activePieces[i].gameObject.SetActive(localX >= leftEdge && localX <= rightEdge);
            }
        }
    }
}