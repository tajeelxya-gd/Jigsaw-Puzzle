using System;
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
        private int _idx;

        public IGroup Group => _groupRef;

        public void SetGroup(IGroup group) => _groupRef = group;

        public void SetColliderSize(Renderer renderer) => _collider.size = renderer.bounds.size;

        public void SetCells(IList<JigsawBoardCell> cells) => _snapController.SetCells(cells);

        public void SetIdx(int idx) => _idx = idx;

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
            _dragController.OnDragEnded += _snapController.SnapToClosestCell;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void OnDestroy()
        {
            _dragController.OnDragged -= Move;
            _dragController.OnDragEnded -= _snapController.SnapToClosestCell;
            _snapController.OnSnapped -= HandleSnapped;

        }

        private void HandleSnapped(int idx)
        {
            var isPlaced = idx == _idx;
            _dragController.enabled = !isPlaced;
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
