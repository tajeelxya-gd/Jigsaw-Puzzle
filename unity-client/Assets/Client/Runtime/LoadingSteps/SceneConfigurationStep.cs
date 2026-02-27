using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase
    {
        [SerializeField] private Transform _camera;

        public override async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            // Wait a frame to ensure correct screen dimensions
            await UniTask.Yield(PlayerLoopTiming.Update);

            SetCameraHeight();
        }

        private void SetCameraHeight()
        {
            float currentWidth = Screen.currentResolution.width;
            float currentHeight = Screen.currentResolution.height;
            float currentAspect = currentWidth / currentHeight;

            // Rest of your logic remains the same
            float refAspect1 = 1920f / 1080f;
            float refAspect2 = 1344f / 2992f;

            float targetY = Mathf.Abs(currentAspect - refAspect1) < Mathf.Abs(currentAspect - refAspect2)
                ? 1.05f : 1.35f;

            Vector3 pos = _camera.localPosition;
            pos.y = targetY;
            _camera.localPosition = pos;
        }
    }
}