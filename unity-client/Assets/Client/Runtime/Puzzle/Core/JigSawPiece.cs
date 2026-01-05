using System;
using System.Collections.Generic;
using UniTx.Runtime.Events;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPiece : MonoBehaviour
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private JigsawPieceVFX _vfx;

        public readonly HashSet<JigSawPiece> Group = new();

        public JigSawPieceData Data { get; private set; }

        public bool IsPlaced { get; private set; }

        public BoxCollider BoxCollider => _collider;

        public PieceSnapController SnapController => _snapController;

        public void Init(JigSawPieceData data)
        {
            Data = data;
            var renderer = Data.Renderer;
            _collider.size = renderer.bounds.size;
            Group.Add(this);
        }

        public void PlayVfx() => _vfx.Play();

        public void Move(Vector3 delta, JigSawPiece toExclude)
        {
            MoveInternal(delta);
            foreach (var piece in Group)
            {
                if (piece == toExclude) continue;
                Move(delta, this);
            }
        }

        public void SetPosY(float y, JigSawPiece toExclude)
        {
            SetPosYInternal(y);
            foreach (var piece in Group)
            {
                if (piece == toExclude) continue;
                SetPosY(y, this);
            }
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

        private void HandleDragStarted() => SetPosY(0.01f, this);

        private void HandleDraggedEnded()
        {
            // if (JoinRegistry.Has())
            // {
            //     var kvps = JoinRegistry.Get();
            //     foreach (var kvp in kvps)
            //     {
            //         var piece = kvp.piece;
            //         var target = kvp.transform;
            //         piece.SnapController.SnapToTransform(target);
            //         Group.Add(piece);
            //     }
            //     JoinRegistry.Clear();
            //     return;
            // }
            _snapController.SnapToClosestCell(Data.Cells);
        }

        private void HandleOnDragged(Vector3 delta) => Move(delta, this);

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
