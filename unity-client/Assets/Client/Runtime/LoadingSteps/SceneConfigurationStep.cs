using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase, IInjectable
    {
        [Header("Reference Settings")]
        [SerializeField] private float _refHeight = 2992;
        [SerializeField] private float _refWidth = 1344;
        [SerializeField] private Vector3 _refPosition = new(0f, 1.11f, -0.23f);
        [SerializeField] private Vector3 _refEulerAngles = new(77f, 0f, 0f);

        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver) => _puzzleService = resolver.Resolve<IPuzzleService>();

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            SetCameraPosition();
            SetPuzzleTrayPosition();
            return UniTask.CompletedTask;
        }

        private void SetCameraPosition()
        {
            var mainCamera = Camera.main.transform;
            var bounds = _puzzleService.PuzzleBounds;
            mainCamera.eulerAngles = _refEulerAngles;

            var refAspect = _refWidth / _refHeight;
            var currentAspect = (float)Screen.width / Screen.height;
            var refOffset = _refPosition - bounds.position;
            var hFactor = refAspect / currentAspect;

            mainCamera.position = bounds.position + (refOffset * hFactor);
        }

        private void SetPuzzleTrayPosition()
        {
        }
    }
}