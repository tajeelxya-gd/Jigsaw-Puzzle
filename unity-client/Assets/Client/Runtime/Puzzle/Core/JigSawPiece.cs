using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(DragController))]
    public sealed class JigSawPiece : MonoBehaviour, IGroupable
    {
        private IGroup _groupRef;
        private DragController _dragController;

        public IGroup Group => _groupRef;

        public JigSawPieceData Data { get; private set; }

        private void Awake()
        {
            _dragController = GetComponent<DragController>();
            _dragController.OnDragged += Move;
        }

        public void SetGroup(IGroup group) => _groupRef = group;

        public void SetData(JigSawPieceData data) => Data = data;

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
