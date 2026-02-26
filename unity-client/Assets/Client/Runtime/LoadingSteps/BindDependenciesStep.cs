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
        [SerializeField] private VFXController _vfxController;
        [SerializeField] private FullImageHandler _fullImageHandler;
        [SerializeField] private SavedDataHandler _savedDataHandler;
        [SerializeField] private ToastHandler _toastHandler;
        [SerializeField] private BoosterPanel _boosterPanel;

        private IBinder _binder;

        public void Inject(IResolver resolver)
        {
            _binder = resolver.Resolve<IBinder>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _binder.Bind<UnityEventListener>();
            _binder.Bind<LocalClock>(); // replace with server clock for prod
            _binder.Bind<ContentService>();
            _binder.Bind<SerialisationService>();
            _binder.Bind<EntityService>();
            _binder.Bind<RewardProcessor>();
            _binder.Bind<CellActionProcessor>();
            _binder.Bind<PuzzleTraySorter>();

            _binder.Bind(_savedDataHandler);
            _binder.Bind<JigsawWinConditionChecker>();
            _binder.Bind(_puzzleService);
            _binder.Bind(_vfxController);
            _binder.Bind(_puzzleTray);
            _binder.Bind(_cameraEffects);
            _binder.Bind(_jigsawResourceLoader);
            _binder.Bind(_fullImageHandler);
            _binder.Bind(_toastHandler);
            _binder.Bind(_boosterPanel);

            return UniTask.CompletedTask;
        }
    }
}