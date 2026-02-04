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
        [SerializeField] private JigsawResourceLoader _jigsawResourceLoader;
        [SerializeField] private HUDComponent _hudComponent;
        [SerializeField] private VFXController _vfxController;
        [SerializeField] private FullImageHandler _fullImageHandler;
        [SerializeField] private SavedDataHandler _savedDataHandler;
        [SerializeField] private ToastHandler _toastHandler;

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
            _binder.BindAsSingleton<RewardProcessor>();
            _binder.BindAsSingleton<CellActionProcessor>();
            _binder.BindAsSingleton<PuzzleTraySorter>();

            _binder.BindAsSingleton(_savedDataHandler);
            _binder.BindAsSingleton<JigsawWinConditionChecker>();
            _binder.BindAsSingleton(_puzzleService);
            _binder.BindAsSingleton(_vfxController);
            _binder.BindAsSingleton(_puzzleTray);
            _binder.BindAsSingleton(_cameraEffects);
            _binder.BindAsSingleton(_jigsawResourceLoader);
            _binder.BindAsSingleton(_hudComponent);
            _binder.BindAsSingleton(_fullImageHandler);
            _binder.BindAsSingleton(_toastHandler);

            return UniTask.CompletedTask;
        }
    }
}