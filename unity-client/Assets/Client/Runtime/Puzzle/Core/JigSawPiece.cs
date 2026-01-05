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

        public readonly HashSet<JigSawPiece> Group = new();

        public JigSawPieceData Data { get; private set; }
        public bool IsPlaced { get; private set; }
        public BoxCollider BoxCollider => _collider;
        public PieceSnapController SnapController => _snapController;

        public void Inject(IResolver resolver)
        {
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public void Init(JigSawPieceData data)
        {
            Data = data;
            var renderer = Data.Renderer;
            _collider.size = renderer.bounds.size;
            Group.Add(this);
        }

        public void PlayVfx() => _vfx.Play();

        /// <summary>
        /// Specifically used when picking the piece from the tray.
        /// Activates the drag controller mid-mouse-press.
        /// </summary>
        public void StartManualDrag()
        {
            _dragController.ForceStartDrag();
        }

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

        private void Awake()
        {
            _dragController.OnDragStarted += HandleDragStarted;
            _dragController.OnDragged += HandleOnDragged;
            _dragController.OnDragEnded += HandleDraggedEnded;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void Update()
        {
            // If this piece is currently being hovered over the tray, 
            // let the tray handle the scale (or do nothing here).
            // Otherwise, if it's not at full scale, return it to 1.0.

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
            return _puzzleTray != null && _puzzleTray.IsOverTray(transform.position);
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
            if (_puzzleTray != null)
            {
                // We set the hover piece regardless; 
                // the tray logic now handles whether it's actually over the collider.
                _puzzleTray.SetHoverPiece(this);
            }
        }

        private void HandleDraggedEnded()
        {
            // Important: Release the tray's hold on this piece first
            if (_puzzleTray != null)
            {
                _puzzleTray.SetHoverPiece(null);
            }

            if (_puzzleTray != null && _puzzleTray.IsOverTray(transform.position))
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
                _dragController.enabled = false;
                _collider.enabled = false;
                _snapController.enabled = false;
                cell.SetPiece(this);
                SetPosYInternal(0f);
                UniEvents.Raise(new PiecePlacedEvent(this));
            }
        }

        private void MoveInternal(Vector3 delta) => transform.position += delta;

        private void SetPosYInternal(float y) => transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}