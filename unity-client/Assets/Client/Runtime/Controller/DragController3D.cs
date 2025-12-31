namespace Client.Runtime
{
    using UnityEngine;

    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public sealed class DragController3D : MonoBehaviour
    {
        private Camera _cam;
        private Rigidbody _rb;

        private Plane _dragPlane;
        private Vector3 _offset;
        private bool _isDragging;

        private void Awake()
        {
            _cam = Camera.main;
            _rb = GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Horizontal plane at piece height (Y-locked)
            _dragPlane = new Plane(Vector3.up, transform.position);
        }

        private void OnMouseDown()
        {
            _isDragging = true;
            _rb.isKinematic = true;

            if (TryGetPlaneHit(out var hit))
                _offset = transform.position - hit;
        }

        private void OnMouseUp()
        {
            _isDragging = false;
            _rb.isKinematic = false;
        }

        private void FixedUpdate()
        {
            if (!_isDragging) return;

            if (TryGetPlaneHit(out var hit))
            {
                Vector3 target = hit + _offset;
                _rb.MovePosition(Vector3.Lerp(
                    _rb.position,
                    target,
                    Time.fixedDeltaTime * 15f
                ));
            }
        }

        private bool TryGetPlaneHit(out Vector3 hitPoint)
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }

            hitPoint = default;
            return false;
        }
    }
}
