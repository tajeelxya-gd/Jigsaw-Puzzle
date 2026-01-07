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

        public void SetActiveJoinController(bool active)
        {
            var joinController = GetJoinController();
            if (joinController != null) joinController.gameObject.SetActive(active);
        }

        public void JoinGroup(JigSawPiece other)
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

        private void OnDestroy()
        {
            _dragController.OnDragStarted -= HandleDragStarted;
            _dragController.OnDragged -= HandleOnDragged;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private bool IsBeingHoveredOverTray()
        {
            if (transform.parent != null) return true;
            return _puzzleTray.IsOverTray(transform.position);
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

            // 1. Physical Tray Check
            bool physicallyOverTray = _puzzleTray.IsOverTray(transform.position);

            // 2. Only submit if it's a single piece
            if (physicallyOverTray && Group.Count == 1)
            {
                _puzzleTray.SubmitPiece(this);
                return;
            }

            // 3. Merge Logic
            if (JoinRegistry.HasCorrectContacts())
            {
                var kvps = JoinRegistry.Flush();
                HashSet<JigSawPiece> processed = new();

                foreach (var kvp in kvps)
                {
                    var join = kvp.join;
                    var piece = kvp.piece;

                    if (processed.Contains(join.Owner)) continue;
                    processed.Add(join.Owner);

                    join.gameObject.SetActive(false);
                    piece.SnapController.SnapToTransform(join.MergeTransform);
                    piece.JoinGroup(join.Owner);
                }
                return;
            }

            _snapController.SnapToClosestCell(Data.Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            IsPlaced = cell.Idx == Data.OriginalIdx;

            if (IsPlaced)
            {
                var groupToLock = Group.ToArray();

                foreach (var piece in groupToLock)
                {
                    piece.IsPlaced = true;
                    var targetCell = Data.Cells.First(c => c.Idx == piece.Data.OriginalIdx);
                    targetCell.SetPiece(piece);
                    piece.LockPiece();
                }

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