using UnityEngine;

public class LookAt : MonoBehaviour
{
    private Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null) return;

        // Rotate object to directly face camera
        transform.LookAt(_cameraTransform);

        // If object's forward is reversed, flip it
        transform.forward = -transform.forward; // Uncomment if facing opposite direction
    }
}