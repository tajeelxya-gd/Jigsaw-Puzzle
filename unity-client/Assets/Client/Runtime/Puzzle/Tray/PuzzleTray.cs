using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleTray : MonoBehaviour, IPuzzleTray
    {
        [Header("Grid Settings")]
        [SerializeField] private int _rowCount = 2;
        [SerializeField] private Vector2 _spacing = new Vector2(0.06f, 0.05f);
        [SerializeField] private Vector2 _padding = new Vector2(0.02f, 0.02f);
        [SerializeField] private BoxCollider _trayCollider;

        [Header("Smoothness Settings")]
        [SerializeField] private float _lerpSpeed = 20f;

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSpeed = 0.15f;
        [SerializeField] private float _visibilityBuffer = 0f;
        [SerializeField] private float _dragThreshold = 10f;

        private readonly List<JigsawPiece> _activePieces = new();
        private JigsawPiece _hitPiece;
        private JigsawPiece _hoverPiece;
        private float _scrollX = 0f;
        private Vector3 _startMousePos;
        private Vector3 _lastMousePos;
        private bool _isDragging;
        private bool _scrollLocked;

        public void ShufflePieces(IEnumerable<JigsawPiece> pieces)
        {
            if (pieces == null) return;

            _activePieces.Clear();
            foreach (var p in pieces)
            {
                p.ScaleDown();
                _activePieces.Add(p);
                p.transform.SetParent(transform);
                p.gameObject.SetActive(true);
            }

            for (int i = 0; i < _activePieces.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, _activePieces.Count);
                (_activePieces[i], _activePieces[randomIndex]) = (_activePieces[randomIndex], _activePieces[i]);
            }

            _scrollX = 0;
        }

        public void SetHoverPiece(JigsawPiece piece) => _hoverPiece = piece;

        public void SubmitPiece(JigsawPiece piece)
        {
            if (piece.Group.Count > 1) return;

            if (!_activePieces.Contains(piece))
            {
                int targetIndex = GetInsertionIndex();
                _activePieces.Insert(Mathf.Clamp(targetIndex, 0, _activePieces.Count), piece);
                piece.transform.SetParent(transform);
            }
            _hoverPiece = null;
        }

        private void Update()
        {
            HandleScrollInput();

            if (!_isDragging)
            {
                ClampScroll();
            }

            UpdatePiecePositions();
        }

        private void UpdatePiecePositions()
        {
            if (_activePieces.Count == 0 && _hoverPiece == null) return;

            // Updated to Middle-Left Anchor
            Vector3 localAnchor = GetLocalMiddleLeftAnchor();

            bool shouldShowInsertionSpace = _hoverPiece != null &&
                _hoverPiece.Group.Count == 1 && _hoverPiece.IsOverTray;

            int insertionIndex = shouldShowInsertionSpace ? GetInsertionIndex() : -1;

            for (int i = 0; i < _activePieces.Count; i++)
            {
                int effectiveIndex = i;

                if (insertionIndex != -1 && i >= insertionIndex)
                {
                    effectiveIndex = i + 1;
                }

                int row = effectiveIndex % _rowCount;
                int col = effectiveIndex / _rowCount;

                Transform pt = _activePieces[i].transform;

                // Position relative to vertical center
                Vector3 targetPos = new Vector3(
                    localAnchor.x + (col * _spacing.x),
                    localAnchor.y,
                    localAnchor.z - (row * _spacing.y)
                );

                pt.localPosition = Vector3.Lerp(pt.localPosition, targetPos, Time.deltaTime * _lerpSpeed);
                pt.localRotation = Quaternion.Lerp(pt.localRotation, Quaternion.identity, Time.deltaTime * _lerpSpeed);

                float localX = pt.localPosition.x;
                float leftEdge = _trayCollider.center.x - (_trayCollider.size.x / 2f) - _visibilityBuffer;
                float rightEdge = _trayCollider.center.x + (_trayCollider.size.x / 2f) + _visibilityBuffer;
                _activePieces[i].gameObject.SetActive(localX >= leftEdge && localX <= rightEdge);
            }
        }

        private int GetInsertionIndex()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 localPoint = transform.InverseTransformPoint(hit.point);
                Vector3 localAnchor = GetLocalMiddleLeftAnchor();

                float relativeX = localPoint.x - (localAnchor.x - _spacing.x * 0.5f);
                // Difference from Z anchor (Top of the centered group)
                float relativeZ = localAnchor.z - localPoint.z;

                int col = Mathf.Max(0, Mathf.FloorToInt(relativeX / _spacing.x));
                int row = Mathf.Clamp(Mathf.FloorToInt(relativeZ / _spacing.y + 0.5f), 0, _rowCount - 1);

                return (col * _rowCount) + row;
            }
            return _activePieces.Count;
        }

        // --- NEW ALIGNMENT LOGIC ---
        private Vector3 GetLocalMiddleLeftAnchor()
        {
            // Calculate how much height the actual rows occupy
            float gridHeight = (_rowCount - 1) * _spacing.y;

            return new Vector3(
                _trayCollider.center.x - (_trayCollider.size.x / 2f) + _padding.x + _scrollX,
                _trayCollider.center.y,
                // Start Z at Center + Half of Grid Height to keep it vertically centered
                _trayCollider.center.z + (gridHeight / 2f)
            );
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

                    if (diffX > _dragThreshold && diffX > diffY) _scrollLocked = true;
                    else if (diffY > _dragThreshold && diffY > diffX)
                    {
                        if (_hitPiece != null) PickUpPiece(_hitPiece);
                        _isDragging = false;
                    }
                }

                if (_scrollLocked)
                {
                    float deltaX = (currentMousePos.x - _lastMousePos.x) / Screen.width;
                    _scrollX += deltaX * _scrollSpeed;
                    ClampScroll();
                }
                _lastMousePos = currentMousePos;
            }
        }

        private void ClampScroll()
        {
            bool countsAsExtra = _hoverPiece != null && _hoverPiece.Group.Count == 1;
            int totalCount = _activePieces.Count + (countsAsExtra ? 1 : 0);

            if (totalCount == 0)
            {
                _scrollX = 0;
                return;
            }

            int cols = Mathf.CeilToInt((float)totalCount / _rowCount);

            float totalContentWidth = (cols - 1) * _spacing.x;
            float visibleWidth = _trayCollider.size.x - (_padding.x * 2);
            float maxScroll = Mathf.Max(0, totalContentWidth - visibleWidth);
            _scrollX = Mathf.Clamp(_scrollX, -maxScroll, 0);
        }

        private void PickUpPiece(JigsawPiece piece)
        {
            _activePieces.Remove(piece);
            piece.transform.SetParent(null);
            piece.StartManualDrag();
        }

        private JigsawPiece GetPieceAtPosition(Vector3 worldPoint)
        {
            if (_activePieces.Count == 0) return null;
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            Vector3 localAnchor = GetLocalMiddleLeftAnchor();

            int col = Mathf.RoundToInt((localPoint.x - localAnchor.x) / _spacing.x);
            int row = Mathf.Clamp(Mathf.RoundToInt((localAnchor.z - localPoint.z) / _spacing.y), 0, _rowCount - 1);

            int index = (col * _rowCount) + row;
            if (index >= 0 && index < _activePieces.Count) return _activePieces[index];
            return null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<JigsawPiece>(out var piece))
            {
                piece.IsOverTray = true;
                piece.ScaleDown();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<JigsawPiece>(out var piece))
            {
                piece.IsOverTray = false;
                piece.ScaleUp();
            }
        }
    }
}