using System.Collections.Generic;
using System.Linq;
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

        private IPuzzleTray _puzzleTray;
        private JigsawBoard _board;
        public JigsawBoardCell CorrectCell { get; private set; } // original cell
        private JigsawBoardCell _currentCell;

        public JigsawGroup Group { get; set; }

        public void Inject(IResolver resolver)
        {
            var puzzleService = resolver.Resolve<IPuzzleService>();
            _board = puzzleService.GetCurrentBoard();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public void Init(JigsawBoardCell cell, JigsawPieceRendererData rendererData)
        {
            _currentCell = null;
            CorrectCell = cell;
            _collider.size = CorrectCell.Size;
            _renderer.Init(rendererData);

            Group = new JigsawGroup()
            {
                this
            };
        }

        public IEnumerable<JigsawBoardCell> GetNeighbours()
            => _currentCell == null ? Enumerable.Empty<JigsawBoardCell>() : _currentCell.GetNeighbours();

        public void PlayVfx() => _vfx.Play();

        public void StartManualDrag() => _dragController.ForceStartDrag();

        public void Move(Vector3 delta)
        {
            foreach (var piece in Group)
            {
                piece.MoveInternal(delta);
            }
        }

        public void SetPosY(float y)
        {
            foreach (var piece in Group)
            {
                piece.SetPosYInternal(y);
            }
        }

        public void JoinGroup(JigsawPiece other)
        {
            if (Group == other.Group) return;

            var groupToKeep = Group.Count >= other.Group.Count ? Group : other.Group;
            var groupToDiscard = (groupToKeep == Group) ? other.Group : Group;

            foreach (var p in groupToDiscard)
            {
                groupToKeep.Add(p);
                p.Group = groupToKeep;
            }

            foreach (var p in groupToKeep)
            {
                p.PlayVfx();
            }
        }

        public void ScaleUp() => _scaleController.ScaleTo(1f);

        public void ScaleDown() => _scaleController.ScaleTo(_board.Data.TrayScaleReduction);

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
            foreach (var piece in Group)
            {
                _currentCell?.Pop();
            }
            SetPosY(0.01f);
        }

        private void HandleOnDragged(Vector3 delta)
        {
            Move(delta);
            _puzzleTray.SetHoverPiece(this);
        }

        private void HandleDraggedEnded()
        {
            _puzzleTray.SetHoverPiece(null);

            // 1. Physical Tray Check
            bool physicallyOverTray = _puzzleTray.IsOverTray(transform.position);

            // 2. Only submit if it's a single piece
            if (physicallyOverTray && Group.Count == 1)
            {
                _puzzleTray.SubmitPiece(this);
                return;
            }

            _snapController.SnapToClosestCell(Group, _board.Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            _currentCell = cell;

            if (cell.Push(this))
            {
                foreach (var piece in Group)
                {
                    piece.CorrectCell.Push(piece);
                    piece._currentCell = piece.CorrectCell;
                    piece.LockPiece();
                }

                UniEvents.Raise(new PiecePlacedEvent(this));

                return;
            }

            var neighbours = GetNeighbours();
            var correctNeighbours = CorrectCell.GetNeighbours();
            foreach (var neighbour in neighbours)
            {
                foreach (var correctNeighbour in correctNeighbours)
                {
                    var piece = neighbour.GetPieceOrDefaultWithIdx(correctNeighbour.Idx);

                    if (piece == null) continue;

                    JoinGroup(piece);
                }

            }
        }

        private void LockPiece()
        {
            _dragController.enabled = false;
            _collider.enabled = false;
            _snapController.enabled = false;
            SetPosYInternal(0f);
            _renderer.SetActive(isFlat: true);
        }

        private void MoveInternal(Vector3 delta) => transform.position += delta;
        private void SetPosYInternal(float y) => transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}