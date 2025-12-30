using UniTx.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Runtime
{
    public sealed class PieceDragController : DragController
    {
        private Vector3 _offset;
        private float _fixedY;
        private Camera _cachedCam;

        protected override void OnDragStarted(PointerEventData eventData)
        {
            UniStatics.LogInfo($"Drag started", this);

            // Cache the camera once per drag session to avoid null checks every frame
            _cachedCam = eventData.pressEventCamera ?? Camera.main;
            _fixedY = transform.position.y;

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
            UniStatics.LogInfo($"Drag ended", this);
            _cachedCam = null;
        }

        private Vector3 GetMouseWorldPosition(Vector2 screenPosition)
        {
            if (_cachedCam == null) return transform.position;

            var groundPlane = new Plane(Vector3.up, new Vector3(0, _fixedY, 0));
            var ray = _cachedCam.ScreenPointToRay(screenPosition);

            return groundPlane.Raycast(ray, out var enter)
                ? ray.GetPoint(enter)
                : transform.position;
        }
    }
}