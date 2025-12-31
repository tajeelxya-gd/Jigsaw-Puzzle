using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(JigSawPiece))]
    [RequireComponent(typeof(DragController))]
    public sealed class PieceSnapController : MonoBehaviour
    {
        [SerializeField] private float snapThreshold = 0.15f;

        private JigSawPiece _piece;
        private DragController _drag;

        private void Awake()
        {
            _piece = GetComponent<JigSawPiece>();
            _drag = GetComponent<DragController>();

            _drag.OnDragEnded += TrySnap;
        }

        private void OnDestroy()
        {
            _drag.OnDragEnded -= TrySnap;
        }

        private void TrySnap()
        {
            if (_piece.Data == null) return;

            foreach (var neighbour in _piece.Data.NeighbourPieces)
            {
                var other = JigSawPieceRegistry.Get(neighbour.Id);
                if (other == null) continue;

                TrySnapTo(other, neighbour.Placement);
            }
        }

        private void TrySnapTo(JigSawPiece other, PlacementDirection placement)
        {
            Vector3 expectedPos = GetExpectedPosition(other, placement);
            float distance = Vector3.Distance(_piece.transform.position, expectedPos);

            if (distance > snapThreshold) return;

            Vector3 delta = expectedPos - _piece.transform.position;
            ApplySnap(delta, other);
        }

        private void ApplySnap(Vector3 delta, JigSawPiece other)
        {
            if (_piece.Group is JigSawGroup group)
                group.Move(delta);
            else
                _piece.transform.position += delta;

            _piece.AttachTo(other.Group ?? CreateGroup(_piece, other));
        }

        private Vector3 GetExpectedPosition(JigSawPiece other, PlacementDirection placement)
        {
            var bounds = other.GetComponent<Renderer>().bounds;
            Vector3 size = bounds.size;

            return placement switch
            {
                PlacementDirection.Top => other.transform.position + new Vector3(0, 0, size.z),
                PlacementDirection.Bottom => other.transform.position + new Vector3(0, 0, -size.z),
                PlacementDirection.Left => other.transform.position + new Vector3(-size.x, 0, 0),
                PlacementDirection.Right => other.transform.position + new Vector3(size.x, 0, 0),
                _ => other.transform.position
            };
        }

        private JigSawGroup CreateGroup(JigSawPiece a, JigSawPiece b)
        {
            var go = new GameObject("JigSawGroup");
            var group = go.AddComponent<JigSawGroup>();

            group.AddMember(a);
            group.AddMember(b);

            return group;
        }
    }
}
