using System;
using UniTx.Runtime.Extensions;
using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(Collider))]
    public sealed class DragController : MonoBehaviour
    {
        public event Action OnDragStarted;
        public event Action<Vector3> OnDragged;
        public event Action OnDragEnded;

        private readonly float _potency = 3.75f;

        private Camera _cam;
        private bool _isDragging;
        private Vector3 _offset;
        private Plane _dragPlane;
        private Vector3 _startPosition;
        private Vector3 _startHitPoint;

        private void Awake() => _cam = Camera.main;

        private void Update()
        {
            HandleInput();
            HandleDrag();
        }

        private void HandleInput()
        {
            if (InputHandler._3DActive && Input.GetMouseButtonDown(0))
            {
                if (TryBeginDrag())
                {
                    BeginDragSequence();
                }
            }

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
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            InitializeDragMath(ray);
            BeginDragSequence();
        }

        private void InitializeDragMath(Ray ray)
        {
            _dragPlane = new Plane(Vector3.up, transform.position);

            if (_dragPlane.Raycast(ray, out float enter))
            {
                _startHitPoint = ray.GetPoint(enter);
                _startPosition = transform.position;
                _offset = _startPosition - _startHitPoint;
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

            Vector3 currentHit = ray.GetPoint(enter);
            Vector3 rawDelta = currentHit - _startHitPoint;

            float zDelta = rawDelta.z;
            float modifiedZ = zDelta > -0.02f ? zDelta * (1f + (zDelta + 0.02f) * _potency) : zDelta;

            Vector3 target = _startPosition + new Vector3(rawDelta.x, 0, modifiedZ);
            Vector3 moveDelta = target - transform.position;
            moveDelta.y = 0f;

            if (moveDelta.sqrMagnitude > Mathf.Epsilon)
            {
                OnDragged.Broadcast(moveDelta);
            }
        }
    }
}