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
        [SerializeField] private Vector3 _refPosition;
        [SerializeField] private Vector3 _refEulerAngles;
        [SerializeField] private Transform _mainCamera;
        [SerializeField] private Transform _puzzleBounds;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            SetCameraPosition();
            return UniTask.CompletedTask;
        }

        [ContextMenu("SetCameraPosition")]
        private void SetCameraPosition()
        {
            _mainCamera.eulerAngles = _refEulerAngles;
            var refAspect = _refWidth / _refHeight;
            var currentAspect = (float)Screen.width / Screen.height;

            // 2. Determine if we need to adjust for "Pillarboxing" (Screen is narrower than reference)
            if (currentAspect < refAspect)
            {
                // Calculate how much we need to "push back" the camera
                // The ratio of aspects tells us the multiplier for the required distance
                var scaleFactor = refAspect / currentAspect;

                // Calculate the distance from reference position to the puzzle center
                var directionToCamera = (_refPosition - _puzzleBounds.position).normalized;
                var refDistance = Vector3.Distance(_refPosition, _puzzleBounds.position);

                // New distance maintains the same horizontal margins
                var newDistance = refDistance * scaleFactor;

                // Set new position along the same look-at vector
                _mainCamera.position = _puzzleBounds.position + (directionToCamera * newDistance);
            }
            else
            {
                // 3. Screen is wider or equal: Use reference position
                // (Vertical margins are naturally preserved by the camera's vertical FOV)
                _mainCamera.position = _refPosition;
            }
        }
    }
}