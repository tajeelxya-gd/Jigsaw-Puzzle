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

        [SerializeField] private float _potency;
        [SerializeField] private float _potencyRate;
        [SerializeField] private float _lerpSpeed = 20f;
        [SerializeField] private float _maxZDelta;
        [SerializeField] private float _startZOffset;

        private Camera _cam;
        private bool _isDragging;
        private Vector3 _offset;
        private Plane _dragPlane;
        private Vector3 _startPosition;
        private Vector3 _startHitPoint;
        private Vector3 _currentSmoothTarget;

        public bool OffsetEnabled { get; set; } = true;

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

        public void SetStartPoints(Vector3 worldPosition, Vector3 hitPoint)
        {
            _startPosition = worldPosition;
            _startHitPoint = hitPoint;
            _dragPlane = new Plane(Vector3.up, _startPosition);
            _offset = _startPosition - _startHitPoint;
            _currentSmoothTarget = transform.position;
        }

        public void ForceStartDrag()
        {
            if (_dragPlane.normal == Vector3.zero) // If not already initialized via SetStartPoints
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                InitializeDragMath(ray);
            }
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
            _startPosition.x = _startHitPoint.x;
            _startPosition.z = _startHitPoint.z + _startZOffset;
            _currentSmoothTarget = transform.position;
            OnDragStarted.Broadcast();
        }

        private void HandleDrag()
        {
            if (!_isDragging) return;

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!_dragPlane.Raycast(ray, out float enter)) return;

            Vector3 currentHit = ray.GetPoint(enter);

            Vector3 instantTarget;
            if (OffsetEnabled)
            {
                Vector3 rawDelta = currentHit - _startHitPoint;
                float zDelta = rawDelta.z;
                float currentPotency = _potency + (zDelta > -0.02f ? (zDelta + 0.02f) * _potencyRate : 0f);
                float modifiedZ = zDelta * currentPotency;
                modifiedZ = Mathf.Min(modifiedZ, zDelta + _maxZDelta);
                instantTarget = _startPosition + new Vector3(rawDelta.x, 0, modifiedZ);
            }
            else
            {
                instantTarget = currentHit;
            }

            _currentSmoothTarget = Vector3.Lerp(_currentSmoothTarget, instantTarget, Time.deltaTime * _lerpSpeed);

            Vector3 moveDelta = _currentSmoothTarget - transform.position;
            moveDelta.y = 0f;

            if (moveDelta.sqrMagnitude > Mathf.Epsilon)
            {
                OnDragged.Broadcast(moveDelta);
            }
        }
    }
}