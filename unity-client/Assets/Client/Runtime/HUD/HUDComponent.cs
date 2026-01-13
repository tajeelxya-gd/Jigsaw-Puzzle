using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class HUDComponent : MonoBehaviour, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Button _reset;
        [SerializeField] private Toggle _cornerPieces;
        [SerializeField] private Toggle _dropPieces;
        [SerializeField] private Toggle _eye;

        private IPuzzleTray _puzzleTray;
        private IJigsawHelper _helper;
        private IWinConditionChecker _checker;

        public void Inject(IResolver resolver)
        {
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _helper = resolver.Resolve<IJigsawHelper>();
            _checker = resolver.Resolve<IWinConditionChecker>();
        }

        public void Initialise() => RegisterEvents();

        public void Reset() => UnRegisterEvents();

        private void RegisterEvents()
        {
            _reset.onClick.AddListener(HandleReset);
            _cornerPieces.onValueChanged.AddListener(HandleCornerPieces);
            _dropPieces.onValueChanged.AddListener(HandleDropPieces);
            _eye.onValueChanged.AddListener(HandleEye);
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            _checker.OnWin += HandleOnWin;
        }

        private void UnRegisterEvents()
        {
            _reset.onClick.RemoveListener(HandleReset);
            _cornerPieces.onValueChanged.RemoveListener(HandleCornerPieces);
            _dropPieces.onValueChanged.RemoveListener(HandleDropPieces);
            _eye.onValueChanged.RemoveListener(HandleEye);
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            _checker.OnWin -= HandleOnWin;
        }

        private void HandleOnWin() => _helper.ShowFullImage(false);

        private void HandleLevelStart(LevelStartEvent @event)
        {
            SetToggles(true);
            _helper.ShowFullImage(false);
        }

        private void HandleReset() => UniWidgets.PushAsync<RestartLevelWidget>();

        private void HandleCornerPieces(bool toggle)
        {
        }

        private void HandleDropPieces(bool toggle) => HandleDropPiecesAsync(toggle, this.GetCancellationTokenOnDestroy()).Forget();

        private async UniTaskVoid HandleDropPiecesAsync(bool toggle, CancellationToken cToken = default)
        {
            if (toggle)
            {
                _puzzleTray.PickPieces();
                return;
            }

            _dropPieces.interactable = false;
            await _puzzleTray.DropPiecesAsync(cToken);
            _dropPieces.interactable = true;
        }

        private void HandleEye(bool toggle) => _helper.ShowFullImage(!toggle);

        private void SetToggles(bool toggle)
        {
            _cornerPieces.SetIsOnWithoutNotify(toggle);
            _dropPieces.SetIsOnWithoutNotify(toggle);
            _eye.SetIsOnWithoutNotify(toggle);
        }
    }
}
