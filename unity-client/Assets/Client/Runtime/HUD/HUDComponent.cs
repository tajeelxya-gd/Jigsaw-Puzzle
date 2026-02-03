using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Audio;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class HUDComponent : MonoBehaviour, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private CurrencyHUDComponent _currencyHudComponent;
        [SerializeField] private Button _reset;
        [SerializeField] private Toggle _dropPieces;
        [SerializeField] private Button _eye;
        [SerializeField] private Button _lock;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _levelName;
        [SerializeField] private ScriptableObject _click;
        [SerializeField] private float _sliderDuration = 0.5f;

        private CancellationTokenSource _sliderCts;

        private IPuzzleTray _puzzleTray;
        private IFullImageHandler _helper;
        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver)
        {
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _helper = resolver.Resolve<IFullImageHandler>();
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _currencyHudComponent.Inject(resolver);
        }

        public void Initialise()
        {
            RegisterEvents();
        }

        public void Reset()
        {
            UnRegisterEvents();
            _sliderCts?.Cancel();
            _sliderCts?.Dispose();
            _sliderCts = null;
        }

        private void RegisterEvents()
        {
            _reset.onClick.AddListener(HandleReset);
            _dropPieces.onValueChanged.AddListener(HandleDropPieces);
            _eye.onClick.AddListener(HandleEye);
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Subscribe<LevelResetEvent>(HandleLevelReset);
            UniEvents.Subscribe<PieceSelectedEvent>(HandlePieceSelected);
            UniEvents.Subscribe<GroupPlacedEvent>(HandleGroupPlaced);
        }

        private void UnRegisterEvents()
        {
            _reset.onClick.RemoveListener(HandleReset);
            _dropPieces.onValueChanged.RemoveListener(HandleDropPieces);
            _eye.onClick.RemoveListener(HandleEye);
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Unsubscribe<LevelResetEvent>(HandleLevelReset);
            UniEvents.Unsubscribe<PieceSelectedEvent>(HandlePieceSelected);
            UniEvents.Unsubscribe<GroupPlacedEvent>(HandleGroupPlaced);
        }

        private void HandlePieceSelected(PieceSelectedEvent @event)
        {
            var targetAlpha = @event.Selected ? 0.2f : 1f;
            _dropPieces.image.SetAlpha(targetAlpha);
            _lock.image.SetAlpha(targetAlpha);
            _eye.image.SetAlpha(targetAlpha);
        }

        private void HandleLevelStart(LevelStartEvent @event)
        {
            _levelName.SetText(_puzzleService.GetCurrentLevelData().Name);
            SetToggles(true);
            SetDropButton(true);
            SetCurrentSliderValue(true);
            _currencyHudComponent.Initialise();
        }

        private void HandleLevelReset(LevelResetEvent @event)
        {
            _currencyHudComponent.Reset();
        }

        private void SetCurrentSliderValue(bool instant = false)
        {
            var pieces = _puzzleService.GetCurrentBoard().Pieces;
            var targetValue = pieces.Count(p => p.IsLocked);
            _slider.maxValue = pieces.Count;

            if (instant)
            {
                _slider.value = targetValue;
                return;
            }

            _sliderCts?.Cancel();
            _sliderCts = new CancellationTokenSource();
            AnimateSliderAsync(targetValue, _sliderCts.Token).Forget();
        }

        private async UniTaskVoid AnimateSliderAsync(float targetValue, CancellationToken cToken = default)
        {
            float startValue = _slider.value;
            float elapsed = 0;

            while (elapsed < _sliderDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _sliderDuration;
                _slider.value = Mathf.Lerp(startValue, targetValue, progress);
                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            _slider.value = targetValue;
        }

        private void HandleGroupPlaced(GroupPlacedEvent @event) => SetCurrentSliderValue();

        private void HandleReset()
        {
            PlayAudio();
            UniWidgets.PushAsync<RestartLevelWidget>();
        }

        private void HandleDropPieces(bool toggle)
        {
            PlayAudio();
            HandleDropPiecesAsync(toggle, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid HandleDropPiecesAsync(bool toggle, CancellationToken cToken = default)
        {
            if (toggle)
            {
                _puzzleTray.PickPieces();
            }
            else
            {
                _dropPieces.interactable = false;
                await _puzzleTray.DropPiecesAsync(cToken);
            }

            SetDropButton(toggle);
        }

        private void HandleEye()
        {
            PlayAudio();
            _helper.ToggleFullImage();
        }

        private void SetToggles(bool toggle)
        {
            _dropPieces.SetIsOnWithoutNotify(toggle);
        }

        private void SetDropButton(bool toggle)
        {
            _dropPieces.transform.localEulerAngles = new Vector3(0f, 0f, toggle ? 0f : 180f);
            _dropPieces.interactable = toggle ? _puzzleTray.CanDropPieces() : _puzzleTray.CanPickPieces();
        }

        private void PlayAudio() => UniAudio.Play2D((IAudioConfig)_click);
    }
}
