using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Clock;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Serialisation;
using UniTx.Runtime.UnityEventListener;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class BindDependenciesStep : LoadingStepBase, IInjectable
    {
        [SerializeField] private PuzzleService _puzzleService;
        [SerializeField] private PuzzleTray _puzzleTray;
        [SerializeField] private CameraEffects _cameraEffects;
        [SerializeField] private JigsawHelper _jigsawHelper;
        [SerializeField] private HUDComponent _hudComponent;
        [SerializeField] private VFXController _vfxController;

        private IBinder _binder;

        public void Inject(IResolver resolver)
        {
            _binder = resolver.Resolve<IBinder>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _binder.BindAsSingleton<UnityEventListener>();
            _binder.BindAsSingleton<LocalClock>(); // replace with server clock for prod
            _binder.BindAsSingleton<ContentService>();
            _binder.BindAsSingleton<SerialisationService>();
            _binder.BindAsSingleton<EntityService>();

            _binder.BindAsSingleton<JigsawWinConditionChecker>();
            _binder.BindAsSingleton(_puzzleService);
            _binder.BindAsSingleton(_vfxController);
            _binder.BindAsSingleton(_puzzleTray);
            _binder.BindAsSingleton(_cameraEffects);
            _binder.BindAsSingleton(_jigsawHelper);
            _binder.BindAsSingleton(_hudComponent);

            return UniTask.CompletedTask;
        }
    }
}