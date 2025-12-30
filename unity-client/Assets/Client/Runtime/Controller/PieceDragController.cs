using UniTx.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PieceDragController : DragController
    {
        private Vector3 _offset;
        private float _fixedY;
        private Camera _cachedCam;

        private Rigidbody _rb;
        private PuzzlePiece _piece;
        private Collider _collider;
        private SnapToSlot _snap;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _piece = GetComponent<PuzzlePiece>();
            _collider = GetComponent<Collider>();
            _snap = GetComponent<SnapToSlot>();
        }

        protected override bool CanStartDrag(PointerEventData eventData)
        {
            if (_piece != null && _piece.IsPlaced)
                return false;

            if (eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            if (eventData.pointerCurrentRaycast.gameObject.layer != LayerMask.NameToLayer("JigsawPiece"))
                return false;

            if (!eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform))
                return false;

            return true;
        }

        protected override void OnDragStarted(PointerEventData eventData)
        {
            _cachedCam = eventData.pressEventCamera ?? Camera.main;
            _fixedY = transform.position.y;

            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            var mouseWorldPos = GetMouseWorldPosition(eventData.position);
            _offset = transform.position - mouseWorldPos;
        }

        protected override void OnDragging(PointerEventData eventData)
        {
            var mouseWorldPos = GetMouseWorldPosition(eventData.position);

            transform.position = new Vector3(
                mouseWorldPos.x + _offset.x,
                _fixedY,
                mouseWorldPos.z + _offset.z
            );
        }

        protected override void OnDragEnded(PointerEventData eventData)
        {
            _rb.isKinematic = false;
            _cachedCam = null;

            // Snap to nearest slot if available
            _snap?.SnapToNearestSlot();
        }

        private Vector3 GetMouseWorldPosition(Vector2 screenPosition)
        {
            if (_cachedCam == null)
                return transform.position;

            var plane = new Plane(Vector3.up, new Vector3(0f, _fixedY, 0f));
            var ray = _cachedCam.ScreenPointToRay(screenPosition);

            return plane.Raycast(ray, out var enter)
                ? ray.GetPoint(enter)
                : transform.position;
        }
    }
}
