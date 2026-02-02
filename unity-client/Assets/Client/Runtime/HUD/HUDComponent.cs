using System;
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
        [SerializeField] private Button _reset;
        [SerializeField] private Toggle _dropPieces;
        [SerializeField] private Button _eye;
        [SerializeField] private Button _lock;
        [SerializeField] private TMP_Text _levelName;
        [SerializeField] private ScriptableObject _click;

        private IPuzzleTray _puzzleTray;
        private IFullImageHandler _helper;
        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver)
        {
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _helper = resolver.Resolve<IFullImageHandler>();
            _puzzleService = resolver.Resolve<IPuzzleService>();
        }

        public void Initialise() => RegisterEvents();

        public void Reset() => UnRegisterEvents();

        private void RegisterEvents()
        {
            _reset.onClick.AddListener(HandleReset);
            _dropPieces.onValueChanged.AddListener(HandleDropPieces);
            _eye.onClick.AddListener(HandleEye);
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Subscribe<PieceSelectedEvent>(HandlePieceSelected);
        }

        private void UnRegisterEvents()
        {
            _reset.onClick.RemoveListener(HandleReset);
            _dropPieces.onValueChanged.RemoveListener(HandleDropPieces);
            _eye.onClick.RemoveListener(HandleEye);
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Unsubscribe<PieceSelectedEvent>(HandlePieceSelected);
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
        }

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
