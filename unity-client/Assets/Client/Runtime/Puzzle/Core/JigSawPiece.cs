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
        [SerializeField] private JigsawPieceRenderer _renderer;

        private IPuzzleService _puzzleService;
        private IPuzzleTray _puzzleTray;

        public JigsawGroup Group { get; set; }
        public JigSawPieceData Data { get; private set; }
        public bool IsLocked { get; private set; }
        public BoxCollider BoxCollider => _collider;
        public PieceSnapController SnapController => _snapController;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public void Init(JigSawPieceData data, JigsawPieceRendererData rendererData)
        {
            Data = data;
            _collider.size = Data.OriginalCell.Size;
            _renderer.Init(rendererData);

            Group = new JigsawGroup($"group_{GetInstanceID()}")
            {
                this
            };
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
            // if (JoinRegistry.HasCorrectContacts())
            // {
            //     var kvps = JoinRegistry.Flush();
            //     HashSet<(string, string)> processedGroups = new();

            //     foreach (var kvp in kvps)
            //     {
            //         var join = kvp.join;
            //         var piece = kvp.piece;
            //         var id1 = piece.Group.Id;
            //         var id2 = join.Owner.Group.Id;

            //         if (id1 == id2) continue;

            //         var pair = string.CompareOrdinal(id1, id2) < 0 ? (id1, id2) : (id2, id1);

            //         if (!processedGroups.Add(pair)) continue;

            //         join.gameObject.SetActive(false);
            //         piece.SnapController.SnapToTransform(join.MergeTransform);
            //         piece.JoinGroup(join.Owner);
            //     }
            //     return;
            // }

            _snapController.SnapToClosestCell(Data.Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            IsLocked = cell.Idx == Data.OriginalCell.Idx;

            if (IsLocked)
            {
                var groupToLock = Group.ToArray();

                foreach (var piece in groupToLock)
                {
                    piece.IsLocked = true;
                    var targetCell = Data.Cells.First(c => c.Idx == piece.Data.OriginalCell.Idx);
                    targetCell.LockPiece(piece);
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
            _renderer.SetActive(isFlat: true);
        }

        private void MoveInternal(Vector3 delta) => transform.position += delta;
        private void SetPosYInternal(float y) => transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}