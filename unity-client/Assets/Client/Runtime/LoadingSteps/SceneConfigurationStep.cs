using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase
    {
        [SerializeField] private float _refHeight;
        [SerializeField] private float _refWidth;
        [SerializeField] private int _refFOV;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _puzzleBounds;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            SetCameraPosition();
            return UniTask.CompletedTask;
        }

        private void SetCameraPosition()
        {
            var refAspect = _refWidth / _refHeight;
            var currentAspect = (float)Screen.width / Screen.height;
            var horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(_refFOV * Mathf.Deg2Rad * 0.5f) * refAspect) * Mathf.Rad2Deg;
            var targetVerticalFOV = 2f * Mathf.Atan(Mathf.Tan(horizontalFOV * Mathf.Deg2Rad * 0.5f) / currentAspect) * Mathf.Rad2Deg;

            _mainCamera.fieldOfView = targetVerticalFOV;
            var cams = GetComponentsInChildren<Camera>();
            foreach (var cam in cams)
            {
                cam.fieldOfView = targetVerticalFOV;
            }
        }
    }
}
