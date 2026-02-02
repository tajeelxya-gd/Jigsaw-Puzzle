using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Audio;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawPiece : MonoBehaviour, IInjectable
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private JigsawPieceVFX _vfx;
        [SerializeField] private JigsawPieceRenderer _renderer;
        [SerializeField] private ScaleController _scaleController;
        [SerializeField] private ScriptableObject _piecePlaced;
        [SerializeField] private ScriptableObject _groupFormed;

        private IPuzzleTray _puzzleTray;
        private IPuzzleService _puzzleService;
        private bool _recentGroup;

        public int CorrectIdx { get; private set; }
        public int CurrentIdx { get; set; }
        public JigsawGroup Group { get; set; }
        public bool IsOverTray { get; private set; }
        public bool IsLocked { get; private set; }
        public JigsawPieceRenderer Renderer => _renderer;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _renderer.Inject(resolver);
        }

        public void Init(JigsawBoardCell cell, JigsawPieceRendererData rendererData)
        {
            CorrectIdx = cell.Idx;
            CurrentIdx = -1;
            _collider.size = cell.Size;
            _renderer.Init(rendererData);

            Group = new JigsawGroup()
            {
                this
            };
        }

        public void PlayVfx() => _vfx.Play();

        public void StartManualDrag() => _dragController.ForceStartDrag();

        public void OnExitTray()
        {
            IsOverTray = false;
            if (Group.Count > 1) return;
            _renderer.OnTrayExit();
            _scaleController.ScaleTo(1f);
        }

        public void OnEnterTray()
        {
            IsOverTray = true;
            if (Group.Count > 1) return;
            _renderer.OnTrayEnter();
            var scale = _puzzleService.GetCurrentBoard().Data.TrayScale;
            _scaleController.ScaleTo(scale);
        }

        public void LockPiece()
        {
            _dragController.enabled = false;
            _collider.enabled = false;
            _snapController.enabled = false;
            IsLocked = true;
        }

        public UniTask SnapToCellAsync(int idx, CancellationToken cToken = default)
        {
            var cells = _puzzleService.GetCurrentBoard().Cells;
            return _snapController.SnapToCellAsync(Group, cells[idx], cToken);
        }

        public UniTask SnapToRandomCellAsync(CancellationToken cToken = default)
        {
            var cells = _puzzleService.GetCurrentBoard().Cells;
            var idx = Random.Range(0, cells.Count);
            return SnapToCellAsync(idx, cToken);
        }

        private void Awake()
        {
            _dragController.OnDragStarted += HandleDragStarted;
            _dragController.OnDragged += HandleOnDragged;
            _dragController.OnDragEnded += HandleDraggedEnded;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void OnDestroy()
        {
            _dragController.OnDragStarted -= HandleDragStarted;
            _dragController.OnDragged -= HandleOnDragged;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private void HandleDragStarted()
        {
            UniEvents.Raise(new PieceSelectedEvent(true));
            _puzzleTray.SetHoverPiece(this);
            Group.SetPosY(0.01f);
            Group.SetHoverShadow();
            Group.SetShadowLayer(LayerMask.NameToLayer("JigsawPiece"));
            Group.RemoveFromCurrentCells();
        }

        private void HandleOnDragged(Vector3 delta) => Group.Move(delta);

        private void HandleDraggedEnded()
        {
            UniEvents.Raise(new PieceSelectedEvent(false));
            _puzzleTray.SetHoverPiece(null);

            if (IsOverTray && Group.Count == 1)
            {
                _puzzleTray.SubmitPiece(this);
                return;
            }

            Group.SetIdleShadow();
            _snapController.SnapToClosestCell(Group, _puzzleService.GetCurrentBoard().Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell, float height)
        {
            Group.SetIdleShadow();
            Group.SetCurrentCells(cell.Idx, this, height);

            if (JigsawBoardCalculator.IsBottomEdge(cell.Idx) || JigsawBoardCalculator.IsLeftEdge(cell.Idx))
            {
                Group.SetShadowLayer(LayerMask.NameToLayer("JigsawFrame"));
            }

            if (cell.Idx == CorrectIdx && cell.Contains(this))
            {
                Group.Lock();
                UniEvents.Raise(new GroupPlacedEvent(Group, this));
                return;
            }

            _recentGroup = false;
            var piecesToCheck = new List<JigsawPiece>(Group);
            foreach (var piece in piecesToCheck)
            {
                if (piece.CurrentIdx == -1) continue;

                var neighbors = JigsawBoardCalculator.GetNeighboursIndices(piece.CurrentIdx);
                CheckAndMerge(piece, neighbors.Top);
                CheckAndMerge(piece, neighbors.Bottom);
                CheckAndMerge(piece, neighbors.Left);
                CheckAndMerge(piece, neighbors.Right);
            }

            var audio = _recentGroup ? _groupFormed : _piecePlaced;
            UniAudio.Play2D((IAudioConfig)audio);
        }

        private void CheckAndMerge(JigsawPiece piece, int neighborIdx)
        {
            if (neighborIdx == -1) return;

            var neighborCell = JigsawBoardCalculator.Board.Cells[neighborIdx];

            foreach (var otherPiece in neighborCell.AllPieces)
            {
                if (otherPiece == null) continue;

                bool isCorrectNeighbor = JigsawBoardCalculator.IsMathematicallyAdjacent(piece, otherPiece);

                if (isCorrectNeighbor && piece.Group != otherPiece.Group)
                {
                    piece.Group.Join(otherPiece.Group);
                    _recentGroup = true;
                    break;
                }
            }
        }
    }
}