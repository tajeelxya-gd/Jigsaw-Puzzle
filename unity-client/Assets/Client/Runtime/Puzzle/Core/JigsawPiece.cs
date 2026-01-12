using System.Collections.Generic;
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
        private IPuzzleService _puzzleService;

        public int CorrectIdx { get; private set; }
        public int CurrentIdx { get; set; }
        public JigsawGroup Group { get; set; }

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
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

        public void ScaleUp()
        {
            if (Group.Count == 1) _scaleController.ScaleTo(1f);
        }

        public void ScaleDown()
        {
            if (Group.Count == 1) _scaleController.ScaleTo(_puzzleService.GetCurrentBoard().Data.TrayScaleReduction);
        }

        public void LockPiece()
        {
            _dragController.enabled = false;
            _collider.enabled = false;
            _snapController.enabled = false;
            _renderer.SetActive(isFlat: true);
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
            _puzzleTray.SetHoverPiece(this);
            Group.SetPosY(0.01f);
            Group.RemoveFromCurrentCells();
        }

        private void HandleOnDragged(Vector3 delta) => Group.Move(delta);

        private void HandleDraggedEnded()
        {
            _puzzleTray.SetHoverPiece(null);

            var physicallyOverTray = _puzzleTray.IsOverTray(transform.position);

            if (physicallyOverTray && Group.Count == 1)
            {
                _puzzleTray.SubmitPiece(this);
                return;
            }

            _snapController.SnapToClosestCell(Group, _puzzleService.GetCurrentBoard().Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            // 1. Update the CurrentIdx for all pieces in the group based on where this piece landed
            Group.SetCurrentCells(cell.Idx, this);

            // 2. If the piece is in its correct home, lock it and exit
            if (cell.Push(this))
            {
                Group.Lock();
                UniEvents.Raise(new GroupPlacedEvent(Group));
                return;
            }

            // 3. Neighbor Merge Check
            // We create a copy of the group to iterate because Join() modifies the collection
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
        }

        private void CheckAndMerge(JigsawPiece piece, int neighborIdx)
        {
            if (neighborIdx == -1) return;

            // Get the cell at the neighbor index
            var neighborCell = JigsawBoardCalculator.Board.Cells[neighborIdx];

            // Check if there is a piece already sitting in that cell
            if (neighborCell.OccupyingPiece != null)
            {
                var otherPiece = neighborCell.OccupyingPiece;

                // If the pieces are mathematically supposed to be neighbors
                // (Checking if their CorrectIdx distance matches their CurrentIdx distance)
                bool isCorrectNeighbor = IsMathematicallyAdjacent(piece, otherPiece);

                if (isCorrectNeighbor && piece.Group != otherPiece.Group)
                {
                    piece.Group.Join(otherPiece.Group);
                }
            }
        }

        private bool IsMathematicallyAdjacent(JigsawPiece a, JigsawPiece b)
        {
            var boardData = JigsawBoardCalculator.Board.Data;
            int cols = boardData.YConstraint;

            // Get 2D grid distance in the "solved" state
            int rA = a.CorrectIdx / cols;
            int cA = a.CorrectIdx % cols;
            int rB = b.CorrectIdx / cols;
            int cB = b.CorrectIdx % cols;

            // Get 2D grid distance in the "current" state
            int curRA = a.CurrentIdx / cols;
            int curCA = a.CurrentIdx % cols;
            int curRB = b.CurrentIdx / cols;
            int curCB = b.CurrentIdx % cols;

            // They are a match if their relative distance is the same in both states
            return (rA - rB == curRA - curRB) && (cA - cB == curCA - curCB);
        }
    }
}