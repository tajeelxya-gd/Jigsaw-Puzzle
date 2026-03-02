using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase
    {
        [SerializeField] private float _fieldOfView;
        [SerializeField] private Camera[] _cameras;

        public override async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            // Wait a frame to ensure correct screen dimensions
            await UniTask.Yield(PlayerLoopTiming.Update);

            AdjustCamera();
        }

        private void AdjustCamera()
        {
            float baseAspect = 9f / 16f;
            float currentAspect = (float)Screen.width / Screen.height;

            if (currentAspect > baseAspect)
            {
                foreach (var camera in _cameras)
                {
                    camera.fieldOfView = _fieldOfView;
                }
            }
            else
            {
                foreach (var camera in _cameras)
                {
                    camera.fieldOfView = _fieldOfView * (baseAspect / currentAspect);
                }
            }
        }
    }
}