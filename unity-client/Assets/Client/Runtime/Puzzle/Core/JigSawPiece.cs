using UniTx.Runtime.Events;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPiece : MonoBehaviour, IGroupable
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;

        private JigSawPieceData _data;

        public IGroup Group { get; private set; }

        public bool IsPlaced { get; private set; }

        public void SetGroup(IGroup group) => Group = group;

        public void Init(JigSawPieceData data)
        {
            _data = data;
            _collider.size = _data.ColliderSize;
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

        public void Highlight(Color color)
        {
            if (Group != null)
            {
                if (Group is JigSawGroup group)
                    group.Highlight(color);
            }
            else
            {
                // single piece highlight
                var renderer = GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = color;
            }
        }

        private void Awake()
        {
            _dragController.OnDragged += Move;
            _dragController.OnDragEnded += HandleDraggedEnded;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void OnDestroy()
        {
            _dragController.OnDragged -= Move;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private void HandleDraggedEnded() => _snapController.SnapToClosestCell(_data.Cells);

        private void HandleSnapped(int idx)
        {
            IsPlaced = idx == _data.OriginalIdx;
            if (IsPlaced)
            {
                _dragController.enabled = false;
                _collider.enabled = false;
                _snapController.enabled = false;
                UniEvents.Raise(new PiecePlacedEvent());
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
    }
}
