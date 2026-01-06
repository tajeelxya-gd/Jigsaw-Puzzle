using System;
using System.Collections.Generic;
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

        public readonly HashSet<JigSawPiece> Group = new();
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
            Group.Add(this);
        }

        public JoinController GetJoinController()
        {
            if (Data.PieceType != PieceType.Join) return null;
            return _joinController ??= GetComponentInChildren<JoinController>(true);
        }

        public void PlayVfx() => _vfx.Play();

        /// <summary>
        /// Specifically used when picking the piece from the tray.
        /// Activates the drag controller mid-mouse-press.
        /// </summary>
        public void StartManualDrag() => _dragController.ForceStartDrag();

        public void Move(Vector3 delta, JigSawPiece toExclude)
        {
            MoveInternal(delta);
            foreach (var piece in Group)
            {
                if (piece == toExclude) continue;
                piece.Move(delta, this);
            }
        }

        public void SetPosY(float y, JigSawPiece toExclude)
        {
            SetPosYInternal(y);
            foreach (var piece in Group)
            {
                if (piece == toExclude) continue;
                piece.SetPosY(y, this);
            }
        }

        public void SetActiveJoinController(bool active)
        {
            var joinController = GetJoinController();
            if (joinController != null) joinController.gameObject.SetActive(active);
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
            // If the piece is already inside the tray (parented), the tray handles it.
            if (transform.parent != null) return true;

            // If we are dragging it and it's over the tray, return true
            return _puzzleTray.IsOverTray(transform.position);
        }

        private void OnDestroy()
        {
            _dragController.OnDragStarted -= HandleDragStarted;
            _dragController.OnDragged -= HandleOnDragged;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private void HandleDragStarted() => SetPosY(0.01f, this);

        private void HandleOnDragged(Vector3 delta)
        {
            Move(delta, this);
            _puzzleTray.SetHoverPiece(this);
        }

        private void HandleDraggedEnded()
        {
            // Important: Release the tray's hold on this piece first
            _puzzleTray.SetHoverPiece(null);

            if (_puzzleTray.IsOverTray(transform.position))
            {
                _puzzleTray.SubmitPiece(this);
            }
            else
            {
                _snapController.SnapToClosestCell(Data.Cells);
            }
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            IsPlaced = cell.Idx == Data.OriginalIdx;
            if (IsPlaced)
            {
                cell.SetPiece(this);
                LockPiece();
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