using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase
    {
        [SerializeField] private Vector3 _boardViewPort;
        [SerializeField] private Vector3 _trayViewPort;
        [SerializeField] private Vector3 _boostersViewPort;
        [SerializeField] private float _fieldOfView;
        [SerializeField] private Camera[] _cameras;
        [SerializeField] private Transform _board;
        [SerializeField] private Transform _tray;
        [SerializeField] private Transform _boosters;

        public override async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            AdjustCamera();
            UpdateViewPortsAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void Update()
        {
            _boosters.position = Camera.main.ViewportToWorldPoint(_boostersViewPort);
        }

        private async UniTask UpdateViewPortsAsync(CancellationToken cToken = default)
        {

            await UniTask.Yield(PlayerLoopTiming.Update);
            _board.position = Camera.main.ViewportToWorldPoint(_boardViewPort);
            _tray.position = Camera.main.ViewportToWorldPoint(_trayViewPort);
            _boosters.position = Camera.main.ViewportToWorldPoint(_boostersViewPort);
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