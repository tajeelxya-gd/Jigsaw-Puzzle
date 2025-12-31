using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(Collider))]
    public sealed class DragController3D : MonoBehaviour
    {
        [SerializeField] private float snapThreshold = 0.2f; // distance to snap
        [SerializeField] private float dragSmoothness = 10f;

        private Camera _cam;
        private JigSawPiece _piece;
        private bool _isDragging;
        private Vector3 _offset;

        private void Awake()
        {
            _cam = Camera.main;
            _piece = GetComponent<JigSawPiece>();
        }

        private void OnMouseDown()
        {
            _isDragging = true;

            // Calculate offset from cursor
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _offset = _piece.transform.position - hit.point;
            }
        }

        private void OnMouseUp()
        {
            _isDragging = false;

            // Try snapping to neighbors after releasing
            TrySnapToNeighbors();
        }

        private void FixedUpdate()
        {
            if (!_isDragging) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                Vector3 target = hit.point + _offset;

                if (_piece.Group != null && _piece.Group is JigSawGroup group)
                {
                    Vector3 delta = target - _piece.transform.position;
                    group.Move(delta); // move entire group
                }
                else
                {
                    _piece.Move(target - _piece.transform.position);
                }
            }
        }

        private void TrySnapToNeighbors()
        {
            foreach (var neighborInfo in _piece.Data.NeighbourPieces)
            {
                var neighborObj = FindPieceById(neighborInfo.Id);
                if (neighborObj == null) continue;

                float distance = Vector3.Distance(_piece.transform.position, neighborObj.transform.position);
                if (distance <= snapThreshold)
                {
                    // Snap to correct position
                    Vector3 snapPos = neighborObj.transform.position + GetOffsetForDirection(neighborInfo.Placement);
                    Vector3 delta = snapPos - _piece.transform.position;
                    _piece.Move(delta);

                    // Merge groups
                    _piece.AttachTo(neighborObj.Group ?? CreateGroup(neighborObj, _piece));
                }
            }
        }

        private JigSawGroup CreateGroup(JigSawPiece a, JigSawPiece b)
        {
            GameObject groupGO = new GameObject("JigsawGroup");
            JigSawGroup group = groupGO.AddComponent<JigSawGroup>();
            group.AddMember(a);
            group.AddMember(b);
            return group;
        }

        private Vector3 GetOffsetForDirection(PlacementDirection dir)
        {
            // assumes pieces are aligned along world axes
            Vector3 offset = Vector3.zero;
            var size = _piece.GetComponent<Renderer>().bounds.size;

            switch (dir)
            {
                case PlacementDirection.Top: offset = new Vector3(0, 0, size.z); break;
                case PlacementDirection.Bottom: offset = new Vector3(0, 0, -size.z); break;
                case PlacementDirection.Left: offset = new Vector3(-size.x, 0, 0); break;
                case PlacementDirection.Right: offset = new Vector3(size.x, 0, 0); break;
            }

            return offset;
        }

        private JigSawPiece FindPieceById(string id)
        {
            var pieces = FindObjectsByType<JigSawPiece>(FindObjectsSortMode.None);
            foreach (var piece in pieces)
            {
                if (piece.Data != null && piece.Data.Id == id)
                    return piece;
            }
            return null;
        }
    }
}
