using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCameraOffset _cinemachineCameraOffset;

    [Header("Reference Resolution (Default 1080x1920 Portrait)")]
    [SerializeField] private float _referenceWidth;
    [SerializeField] private float _referenceHeight;
    [SerializeField] private float _referenceZOffset;

    private float _referenceAspect;

    private void Awake()
    {
        // if (_cinemachineCameraOffset == null)
        //     _cinemachineCameraOffset = GetComponent<CinemachineCameraOffset>();

        // _referenceAspect = _referenceWidth / _referenceHeight;

        // AdjustZOffset();
    }
    [Button]
    private void AdjustZOffset()
    {
        if (Application.isEditor) return;
        if (_cinemachineCameraOffset == null) return;

        float currentAspect = (float)Screen.width / Screen.height;
        float scale = currentAspect / _referenceAspect;
        Vector3 newOffset = _cinemachineCameraOffset.Offset;
        newOffset.z = _referenceZOffset / scale;

        _cinemachineCameraOffset.Offset = newOffset;
    }
}
