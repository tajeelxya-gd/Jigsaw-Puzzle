using System;
using UniTx.Runtime.Extensions;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(Collider))]
    public sealed class DragController : MonoBehaviour
    {
        public event Action OnDragStarted;
        public event Action<Vector3> OnDragged;   // delta
        public event Action OnDragEnded;

        private Camera _cam;
        private bool _isDragging;
        private Vector3 _offset;
        private Plane _dragPlane;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            HandleInput();
            HandleDrag();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (TryBeginDrag())
                {
                    _isDragging = true;
                    OnDragStarted.Broadcast();
                }
            }

            if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                _isDragging = false;
                OnDragEnded.Broadcast();
            }
        }

        private bool TryBeginDrag()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return false;
            if (hit.transform != transform) return false;

            // Lock plane at drag start (2D board plane)
            _dragPlane = new Plane(Vector3.up, transform.position);

            if (!_dragPlane.Raycast(ray, out float enter)) return false;

            Vector3 planeHit = ray.GetPoint(enter);
            _offset = transform.position - planeHit;

            return true;
        }

        private void HandleDrag()
        {
            if (!_isDragging) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!_dragPlane.Raycast(ray, out float enter)) return;

            Vector3 planeHit = ray.GetPoint(enter);
            Vector3 target = planeHit + _offset;

            Vector3 delta = target - transform.position;

            // 2D-style constraint (XZ board)
            delta.y = 0f;

            OnDragged.Broadcast(delta);
        }
    }
}
