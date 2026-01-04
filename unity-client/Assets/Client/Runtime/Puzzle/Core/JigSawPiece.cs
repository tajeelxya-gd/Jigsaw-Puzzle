using System;
using UniTx.Runtime.Events;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPiece : MonoBehaviour, IGroupable
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private JigsawPieceVFX _vfx;

        public JigSawPieceData Data { get; private set; }

        public IGroup Group { get; private set; }

        public bool IsPlaced { get; private set; }

        public void SetGroup(IGroup group) => Group = group;

        public BoxCollider BoxCollider => _collider;

        public void Init(JigSawPieceData data)
        {
            Data = data;
            var renderer = Data.Renderer;
            _collider.size = renderer.bounds.size;
        }

        public void AttachTo(IGroup other)
        {
            if (Group == null && other != null)
            {
                other.AddMember(this);
            }
            else if (Group != null && other != null && Group != other)
            {
                Group.Merge(other);
            }
        }

        public void PlayVfx() => _vfx.Play();

        private void Awake()
        {
            _dragController.OnDragStarted += HandleDragStarted;
            _dragController.OnDragged += Move;
            _dragController.OnDragEnded += HandleDraggedEnded;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void OnDestroy()
        {
            _dragController.OnDragStarted -= HandleDragStarted;
            _dragController.OnDragged -= Move;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private void HandleDragStarted() => SetPosY(0.01f);

        private void HandleDraggedEnded() => _snapController.SnapToClosestCell(Data.Cells);

        private void HandleSnapped(JigsawBoardCell cell)
        {
            IsPlaced = cell.Idx == Data.OriginalIdx;
            if (IsPlaced)
            {
                _dragController.enabled = false;
                _collider.enabled = false;
                _snapController.enabled = false;
                cell.SetPiece(this);
                SetPosY(0f);
                UniEvents.Raise(new PiecePlacedEvent(this));
            }
        }

        private void Move(Vector3 delta)
        {
            if (Group != null)
            {
                // delegate to group
                if (Group is JigSawGroup group)
                    group.Move(delta);
            }
            else
            {
                // single piece move
                transform.position += delta;
            }
        }

        private void SetPosY(float y) => transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
