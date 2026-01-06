using System;
using System.Collections.Generic;
using System.Linq;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPiece : MonoBehaviour, IInjectable
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private JigsawPieceVFX _vfx;

        private IPuzzleTray _puzzleTray;
        private JoinController _joinController;

        // Note: Removed 'readonly' so we can swap references during merging
        public HashSet<JigSawPiece> Group { get; set; } = new();
        public JigSawPieceData Data { get; private set; }
        public bool IsPlaced { get; private set; }
        public BoxCollider BoxCollider => _collider;
        public PieceSnapController SnapController => _snapController;

        public void Inject(IResolver resolver) => _puzzleTray = resolver.Resolve<IPuzzleTray>();

        public void Init(JigSawPieceData data)
        {
            Data = data;
            var renderer = Data.Renderer;
            _collider.size = renderer.bounds.size;

            // Ensure the group contains at least this piece
            Group.Clear();
            Group.Add(this);
        }

        public JoinController GetJoinController()
        {
            if (Data.PieceType != PieceType.Join) return null;
            return _joinController ??= GetComponentInChildren<JoinController>(true);
        }

        public void PlayVfx() => _vfx.Play();

        public void StartManualDrag() => _dragController.ForceStartDrag();

        /// <summary>
        /// Moves the entire group by iterating through the shared HashSet.
        /// Calling MoveInternal prevents the StackOverflow recursion.
        /// </summary>
        public void Move(Vector3 delta)
        {
            foreach (var piece in Group)
            {
                piece.MoveInternal(delta);
            }
        }

        /// <summary>
        /// Sets the Y position for the entire group.
        /// </summary>
        public void SetPosY(float y)
        {
            foreach (var piece in Group)
            {
                piece.SetPosYInternal(y);
            }
        }

        public void SetActiveJoinController(bool active)
        {
            var joinController = GetJoinController();
            if (joinController != null) joinController.gameObject.SetActive(active);
        }

        public void JoinGroup(JigSawPiece other)
        {
            // Already in the same group instance
            if (this.Group == other.Group) return;

            // Merge the smaller group into the larger group for performance
            HashSet<JigSawPiece> groupToKeep = this.Group.Count >= other.Group.Count ? this.Group : other.Group;
            HashSet<JigSawPiece> groupToDiscard = (groupToKeep == this.Group) ? other.Group : this.Group;

            foreach (JigSawPiece p in groupToDiscard)
            {
                groupToKeep.Add(p);
                p.Group = groupToKeep;
            }
        }

        private void Awake()
        {
            _dragController.OnDragStarted += HandleDragStarted;
            _dragController.OnDragged += HandleOnDragged;
            _dragController.OnDragEnded += HandleDraggedEnded;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void Update()
        {
            if (_puzzleTray == null) return;
            if (IsBeingHoveredOverTray()) return;

            if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.Lerp(
                    transform.localScale,
                    Vector3.one,
                    Time.deltaTime * 10f
                );
            }
        }

        private bool IsBeingHoveredOverTray()
        {
            if (transform.parent != null) return true;
            return _puzzleTray.IsOverTray(transform.position);
        }

        private void OnDestroy()
        {
            _dragController.OnDragStarted -= HandleDragStarted;
            _dragController.OnDragged -= HandleOnDragged;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private void HandleDragStarted() => SetPosY(0.01f);

        private void HandleOnDragged(Vector3 delta)
        {
            Move(delta);
            _puzzleTray.SetHoverPiece(this);
        }

        private void HandleDraggedEnded()
        {
            _puzzleTray.SetHoverPiece(null);

            // 1. Only single pieces can enter the tray
            // If it's a cluster, we skip tray logic entirely and snap to grid
            if (Group.Count == 1 && _puzzleTray.IsOverTray(transform.position))
            {
                _puzzleTray.SubmitPiece(this);
                return;
            }

            // 2. Check for merging with other pieces in the world
            if (JoinRegistry.HasCorrectContacts())
            {
                var kvps = JoinRegistry.Get().ToArray();
                JoinRegistry.Clear();
                foreach (var kvp in kvps)
                {
                    var join = kvp.join;
                    var piece = kvp.piece;

                    piece.SnapController.SnapToTransform(join.MergeTransform);
                    piece.JoinGroup(this);
                }
                return;
            }

            // 3. Default behavior: Snap cluster or single piece to the closest board cell
            // This handles the "cluster dropped on tray" case by snapping it to the board instead
            _snapController.SnapToClosestCell(Data.Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            // Check if the piece we just dropped is in its correct slot
            IsPlaced = cell.Idx == Data.OriginalIdx;

            if (IsPlaced)
            {
                // 1. Create a temporary list/array from the group 
                // We do this because LockPiece might change group state or disabling components
                var groupToLock = Group.ToArray();

                foreach (var piece in groupToLock)
                {
                    // 2. Mark each piece as placed
                    piece.IsPlaced = true;

                    // 3. Find the cell this specific group-piece belongs to.
                    // Since they are snapped perfectly, we can find the cell by its OriginalIdx
                    var targetCell = Data.Cells.FirstOrDefault(c => c.Idx == piece.Data.OriginalIdx);

                    if (targetCell != null)
                    {
                        targetCell.SetPiece(piece);
                    }

                    // 4. Disable physics/input for this piece
                    piece.LockPiece();
                }

                // 5. Raise the event once for the whole cluster
                UniEvents.Raise(new PiecePlacedEvent(this));
            }
        }

        private void LockPiece()
        {
            _dragController.enabled = false;
            _collider.enabled = false;
            _snapController.enabled = false;
            SetPosYInternal(0f);
            SetActiveJoinController(false);
        }

        private void MoveInternal(Vector3 delta) => transform.position += delta;
        private void SetPosYInternal(float y) => transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}