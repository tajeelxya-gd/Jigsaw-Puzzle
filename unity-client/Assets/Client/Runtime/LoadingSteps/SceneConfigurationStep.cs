using System;
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
        [SerializeField] private BoxCollider _trayCollider;

        public override async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            // AdjustCamera();
            // UpdateViewPortsAsync(this.GetCancellationTokenOnDestroy()).Forget();
            SetTrayCollider();
        }

        private async UniTask UpdateViewPortsAsync(CancellationToken cToken = default)
        {

            await UniTask.Yield(PlayerLoopTiming.Update);
            _board.position = Camera.main.ViewportToWorldPoint(_boardViewPort);
            _tray.position = Camera.main.ViewportToWorldPoint(_trayViewPort);
            _boosters.position = Camera.main.ViewportToWorldPoint(_boostersViewPort);
            SetTrayCollider();
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

        private void SetTrayCollider()
        {
            if (_trayCollider == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            // Get viewport boundaries transformed to world space at the tray's depth
            Vector3 leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, _trayViewPort.y, _trayViewPort.z));
            Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, _trayViewPort.y, _trayViewPort.z));

            float worldWidth = Vector3.Distance(leftEdge, rightEdge);
            Vector3 worldCenter = (leftEdge + rightEdge) * 0.5f;

            // Update collider size (X dimension only)
            Vector3 size = _trayCollider.size;
            size.x = worldWidth / _trayCollider.transform.lossyScale.x;
            _trayCollider.size = size;

            // Update collider center (X dimension only) to align with screen center
            Vector3 localCenter = _trayCollider.transform.InverseTransformPoint(worldCenter);
            Vector3 colliderCenter = _trayCollider.center;
            colliderCenter.x = localCenter.x;
            _trayCollider.center = colliderCenter;
        }
    }
}