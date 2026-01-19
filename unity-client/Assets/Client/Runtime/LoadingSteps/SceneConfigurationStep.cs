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
        [SerializeField] private RectTransform _boardSection;
        [SerializeField] private RectTransform _puzzleTraySection;

        private IPuzzleService _puzzleService;
        private IPuzzleTray _puzzleTray;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            var boardRoot = _puzzleService.PuzzleBoard;
            var boardBounds = _puzzleService.PuzzleBounds;
            var trayRoot = _puzzleTray.TrayCollider.transform;
            var trayMesh = _puzzleTray.MeshTransform;

            // Do stuff

            return UniTask.CompletedTask;
        }
    }
}