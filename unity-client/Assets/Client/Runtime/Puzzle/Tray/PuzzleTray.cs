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
        [SerializeField] private float _lerpSpeed = 10f;

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSpeed = 0.1f;
        [SerializeField] private float _visibilityBuffer = 0.01f;
        [SerializeField] private float _dragThreshold = 10f;

        private readonly List<JigSawPiece> _activePieces = new();
        private JigSawPiece _hitPiece;
        private JigSawPiece _hoverPiece;

        private float _scrollX = 0f;
        private Vector3 _startMousePos;
        private Vector3 _lastMousePos;
        private bool _isDragging;
        private bool _scrollLocked;

        public void ShufflePieces(IReadOnlyList<JigSawPiece> pieces)
        {
            if (pieces == null || pieces.Count == 0) return;
            _activePieces.Clear();
            foreach (var p in pieces)
            {
                _activePieces.Add(p);
                p.transform.SetParent(transform);
                p.gameObject.SetActive(true);
            }
            for (int i = 0; i < _activePieces.Count; i++)
            {
                int randomIndex = Random.Range(i, _activePieces.Count);
                (_activePieces[i], _activePieces[randomIndex]) = (_activePieces[randomIndex], _activePieces[i]);
            }
            _scrollX = 0;
        }

        public bool IsOverTray(Vector3 worldPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit) && hit.collider == _trayCollider;
        }

        public void SetHoverPiece(JigSawPiece piece) => _hoverPiece = piece;

        public void SubmitPiece(JigSawPiece piece)
        {
            if (!_activePieces.Contains(piece))
            {
                int targetIndex = GetInsertionIndex();
                _activePieces.Insert(Mathf.Clamp(targetIndex, 0, _activePieces.Count), piece);
                piece.transform.SetParent(transform);
            }
            // This nullifies the reference so HandleHoverPieceVisuals() stops running
            _hoverPiece = null;
        }

        private void Update()
        {
            HandleScrollInput();
            UpdatePiecePositions();
            HandleHoverPieceVisuals();
        }

        private void HandleHoverPieceVisuals()
        {
            if (_hoverPiece == null) return;

            // The Tray only forces a scale IF the piece is within the tray bounds
            bool isOver = IsOverTray(_hoverPiece.transform.position);

            if (isOver)
            {
                Vector3 targetScale = Vector3.one * _scaleReduction;
                _hoverPiece.transform.localScale = Vector3.Lerp(
                    _hoverPiece.transform.localScale,
                    targetScale,
                    Time.deltaTime * _lerpSpeed
                );
            }
            // Note: We REMOVED the 'else scale to 1.0' here. 
            // The Piece's own Update now handles the return to 1.0.
        }

        private void UpdatePiecePositions()
        {
            if (_activePieces.Count == 0 && _hoverPiece == null) return;

            Vector3 localTopLeft = GetLocalTopLeft();

            // Shifting only happens if we are currently dragging a piece over the tray
            int insertionIndex = (_hoverPiece != null && IsOverTray(_hoverPiece.transform.position))
                ? GetInsertionIndex()
                : -1;

            int totalCount = _activePieces.Count + (insertionIndex != -1 ? 1 : 0);
            int cols = Mathf.CeilToInt((float)totalCount / _rowCount);

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
                Vector3 targetPos = new Vector3(
                    localTopLeft.x + (col * _spacing.x),
                    localTopLeft.y,
                    localTopLeft.z - (row * _spacing.y)
                );

                pt.localPosition = Vector3.Lerp(pt.localPosition, targetPos, Time.deltaTime * _lerpSpeed);
                pt.localScale = Vector3.Lerp(pt.localScale, Vector3.one * _scaleReduction, Time.deltaTime * _lerpSpeed);
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
                Vector3 localTopLeft = GetLocalTopLeft();

                float relativeX = localPoint.x - (localTopLeft.x - _spacing.x * 0.5f);
                float relativeZ = localTopLeft.z - localPoint.z;

                int col = Mathf.Max(0, Mathf.FloorToInt(relativeX / _spacing.x));
                int row = Mathf.Clamp(Mathf.FloorToInt(relativeZ / _spacing.y), 0, _rowCount - 1);

                return (col * _rowCount) + row;
            }
            return _activePieces.Count;
        }

        private Vector3 GetLocalTopLeft()
        {
            return new Vector3(
                _trayCollider.center.x - (_trayCollider.size.x / 2f) + _padding.x + _scrollX,
                _trayCollider.center.y,
                _trayCollider.center.z + (_trayCollider.size.z / 2f) - _padding.y
            );
        }

        #region Input Logic
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
            int totalCount = _activePieces.Count + (_hoverPiece != null ? 1 : 0);
            int cols = Mathf.CeilToInt((float)totalCount / _rowCount);
            float totalWidth = (cols - 1) * _spacing.x;
            float maxScroll = Mathf.Max(0, totalWidth - _trayCollider.size.x + (_padding.x * 2));
            _scrollX = Mathf.Clamp(_scrollX, -maxScroll, 0);
        }

        private void PickUpPiece(JigSawPiece piece)
        {
            _activePieces.Remove(piece);
            piece.transform.SetParent(null);
            piece.StartManualDrag();
        }

        private JigSawPiece GetPieceAtPosition(Vector3 worldPoint)
        {
            if (_activePieces.Count == 0) return null;
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            Vector3 localTopLeft = GetLocalTopLeft();

            int col = Mathf.RoundToInt((localPoint.x - localTopLeft.x) / _spacing.x);
            int row = Mathf.Clamp(Mathf.RoundToInt((localTopLeft.z - localPoint.z) / _spacing.y), 0, _rowCount - 1);

            int index = (col * _rowCount) + row;
            if (index >= 0 && index < _activePieces.Count) return _activePieces[index];
            return null;
        }
        #endregion
    }
}