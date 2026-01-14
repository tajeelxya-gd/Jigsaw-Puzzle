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
            // Standard click-to-drag logic
            if (InputHandler._3DActive && Input.GetMouseButtonDown(0))
            {
                if (TryBeginDrag())
                {
                    BeginDragSequence();
                }
            }

            // End drag when mouse is released
            if (InputHandler._3DActive && Input.GetMouseButtonUp(0) && _isDragging)
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

            InitializeDragMath(ray);
            return true;
        }

        public void ForceStartDrag()
        {
            // Ensure we are at full scale when dragging starts
            transform.localScale = Vector3.one;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            InitializeDragMath(ray);
            BeginDragSequence();
        }

        private void InitializeDragMath(Ray ray)
        {
            // Lock plane at drag start (XZ board plane)
            _dragPlane = new Plane(Vector3.up, transform.position);

            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 planeHit = ray.GetPoint(enter);
                _offset = transform.position - planeHit;
            }
        }

        private void BeginDragSequence()
        {
            _isDragging = true;
            OnDragStarted.Broadcast();
        }

        private void HandleDrag()
        {
            if (!_isDragging) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!_dragPlane.Raycast(ray, out float enter)) return;

            Vector3 planeHit = ray.GetPoint(enter);
            Vector3 target = planeHit + _offset;
            Vector3 delta = target - transform.position;
            delta.y = 0f;

            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                OnDragged.Broadcast(delta);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            }
        }
    }
}