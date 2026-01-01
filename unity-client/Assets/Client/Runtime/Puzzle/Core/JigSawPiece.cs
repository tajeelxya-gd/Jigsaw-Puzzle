using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawPiece : MonoBehaviour, IGroupable
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;

        private IGroup _groupRef;

        public IGroup Group => _groupRef;

        public JigSawPieceData Data { get; private set; }

        public void SetGroup(IGroup group) => _groupRef = group;

        public void SetData(JigSawPieceData data) => Data = data;

        public void SetColliderSize(Renderer renderer) => _collider.size = renderer.bounds.size;

        public void SetPlacements(IList<Transform> placements) => _snapController.SetPlacements(placements);

        public void AttachTo(IGroup other)
        {
            if (_groupRef == null && other != null)
            {
                other.AddMember(this);
            }
            else if (_groupRef != null && other != null && _groupRef != other)
            {
                _groupRef.Merge(other);
            }
        }

        public void Highlight(Color color)
        {
            if (_groupRef != null)
            {
                if (_groupRef is JigSawGroup group)
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
            _dragController.OnDragEnded += _snapController.SnapToClosestPlacement;
        }

        private void OnDestroy()
        {
            _dragController.OnDragged -= Move;
            _dragController.OnDragEnded -= _snapController.SnapToClosestPlacement;
        }

        private void Move(Vector3 delta)
        {
            if (_groupRef != null)
            {
                // delegate to group
                if (_groupRef is JigSawGroup group)
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
