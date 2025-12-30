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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _piece = GetComponent<PuzzlePiece>();
        }

        protected override bool CanStartDrag(PointerEventData eventData)
        {
            if (_piece != null && _piece.IsPlaced)
                return false;

            // Must actually hit THIS object (or its children)
            if (eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            if (!eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform))
                return false;

            return true;
        }

        protected override void OnDragStarted(PointerEventData eventData)
        {
            UniStatics.LogInfo("Piece drag started", this);

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
            UniStatics.LogInfo("Piece drag ended", this);

            _rb.isKinematic = false;
            _cachedCam = null;
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
