using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleTray : MonoBehaviour, IPuzzleTray, IResettable, IInjectable
    {
        [Header("Grid Settings")]
        [SerializeField] private int _rowCount = 2;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private Vector2 _padding = new Vector2(0.02f, 0.02f);
        [SerializeField] private BoxCollider _trayCollider;
        [SerializeField] private Transform _bgTransform;

        [Header("Smoothness Settings")]
        [SerializeField] private float _lerpSpeed = 20f;

        [Header("Scroll Settings")]
        [SerializeField] private float _scrollSpeed = 0.15f;
        [SerializeField] private float _visibilityBuffer = 0f;
        [SerializeField] private float _dragThreshold = 10f;
        [SerializeField] private float _decelerationRate = 0.95f;

        private readonly List<JigsawPiece> _activePieces = new();
        private Camera _cam;
        private IPuzzleService _puzzleService;
        private JigsawPiece _hitPiece;
        private JigsawPiece _hoverPiece;
        private float _scrollX = 0f;
        private Vector3 _startMousePos;
        private Vector3 _lastMousePos;
        private bool _isDragging;
        private bool _scrollLocked;
        private float _scrollVelocity = 0f;

        public BoxCollider TrayCollider => _trayCollider;

        public Transform BgTransform => _bgTransform;

        public void Inject(IResolver resolver) => _puzzleService = resolver.Resolve<IPuzzleService>();

        public void Reset()
        {
            _activePieces.Clear();
            _scrollX = 0;
            _scrollVelocity = 0f;
            _hitPiece = null;
            _hoverPiece = null;
            _startMousePos = _lastMousePos = Vector3.zero;
            _isDragging = _scrollLocked = false;
        }

        public void ShufflePieces(IEnumerable<JigsawPiece> pieces)
        {
            SetSpacing();
            if (pieces == null) return;

            _activePieces.Clear();
            foreach (var p in pieces)
            {
                p.OnEnterTray();
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
            _scrollVelocity = 0f;
        }

        public void SetHoverPiece(JigsawPiece piece)
        {
            _hoverPiece = piece;
            if (_hoverPiece != null)
            {
                // Force an immediate check so the scale updates the frame it's picked up
                HandleHoverPieceTrayState();
            }
        }

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

        public UniTask DropPiecesAsync(CancellationToken cToken = default)
        {
            var tasks = new List<UniTask>();
            while (_activePieces.Count > 0)
            {
                var piece = _activePieces[0];
                _activePieces.Remove(piece);
                piece.transform.SetParent(null);
                piece.gameObject.SetActive(true);
                piece.OnExitTray();
                tasks.Add(piece.SnapToRandomCellAsync(cToken));
            }
            _scrollX = 0;
            _scrollVelocity = 0f;
            return UniTask.WhenAll(tasks);
        }

        public void PickPieces()
        {
            var pieces = _puzzleService.GetCurrentBoard().Pieces;

            foreach (var piece in pieces)
            {
                if (piece.IsLocked || piece.IsOverTray) continue;
                var group = piece.Group;
                if (group.Count > 1) continue;
                group.RemoveFromCurrentCells();
                piece.OnEnterTray();
                SubmitPiece(piece);
            }
        }

        public bool CanDropPieces() => _activePieces.Count > 0;

        public bool CanPickPieces()
        {
            var pieces = _puzzleService.GetCurrentBoard().Pieces;

            return pieces.Any(itm => !itm.IsLocked && !itm.IsOverTray && itm.Group.Count == 1);
        }

        private void Awake() => _cam = Camera.main;

        private void Update()
        {
            HandleScrollInput();
            HandleHoverPieceTrayState();

            if (!_isDragging)
            {
                ApplyInertia();
                ClampScroll();
            }

            UpdatePiecePositions();
        }

        private void ApplyInertia()
        {
            if (Mathf.Abs(_scrollVelocity) > 0.001f)
            {
                _scrollX += _scrollVelocity * Time.deltaTime;
                _scrollVelocity *= Mathf.Pow(_decelerationRate, Time.deltaTime * 60f);
            }
            else
            {
                _scrollVelocity = 0f;
            }
        }

        private void SetSpacing()
        {
            var boardData = _puzzleService.GetCurrentBoard().Data;
            _spacing = boardData.TraySpacing;
        }

        private void HandleHoverPieceTrayState()
        {
            if (_hoverPiece == null) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            // We use Raycast specifically on _trayCollider to ignore the piece itself 
            // blocking the ray, and to avoid physics scaling flicker.
            bool isOver = _trayCollider.Raycast(ray, out _, 100f);

            if (isOver && !_hoverPiece.IsOverTray)
            {
                _hoverPiece.OnEnterTray();
            }
            else if (!isOver && _hoverPiece.IsOverTray)
            {
                _hoverPiece.OnExitTray();
            }
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
                // _activePieces[i].gameObject.SetActive(localX >= leftEdge && localX <= rightEdge);
            }
        }

        private int GetInsertionIndex()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
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
            if (InputHandler._3DActive && Input.GetMouseButtonDown(0))
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == _trayCollider)
                {
                    _isDragging = true;
                    _scrollLocked = false;
                    _scrollVelocity = 0f;
                    _startMousePos = Input.mousePosition;
                    _lastMousePos = Input.mousePosition;
                    _hitPiece = GetPieceAtPosition(hit.point);
                }
            }

            if (InputHandler._3DActive && Input.GetMouseButtonUp(0))
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
                    Vector2 delta = currentMousePos - _startMousePos;
                    float totalDist = delta.magnitude;

                    if (totalDist > _dragThreshold)
                    {
                        float angle = Mathf.Abs(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

                        bool isHorizontal = angle < 25f || angle > 155f;

                        if (isHorizontal)
                        {
                            _scrollLocked = true;
                        }
                        else
                        {
                            if (_hitPiece != null) PickUpPiece(_hitPiece);
                            _isDragging = false;
                        }
                    }
                }

                if (_scrollLocked)
                {
                    float deltaX = (currentMousePos.x - _lastMousePos.x) / Screen.width;
                    float scrollDelta = deltaX * _scrollSpeed;
                    _scrollX += scrollDelta;
                    _scrollVelocity = scrollDelta / Time.deltaTime;
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

            float clampedX = Mathf.Clamp(_scrollX, -maxScroll, 0);
            if (!Mathf.Approximately(clampedX, _scrollX))
            {
                _scrollX = clampedX;
                _scrollVelocity = 0f;
            }
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
    }
}