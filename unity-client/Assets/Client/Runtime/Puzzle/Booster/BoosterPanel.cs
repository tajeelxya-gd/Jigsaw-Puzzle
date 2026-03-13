using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class BoosterPanel : MonoBehaviour, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private int _magnetPiecesCount;
        [SerializeField] private Button _eyeButton;
        [SerializeField] private Button _magnetButton;
        [SerializeField] private TMP_Text _eyeCount;
        [SerializeField] private TMP_Text _magnetCount;

        private IFullImageHandler _fullImageHandler;
        private IPuzzleTray _puzzleTray;
        private IWinConditionChecker _winConditionChecker;

        public void Inject(IResolver resolver)
        {
            _fullImageHandler = resolver.Resolve<IFullImageHandler>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _winConditionChecker = resolver.Resolve<IWinConditionChecker>();
        }

        public void Initialise()
        {
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            _eyeButton.onClick.AddListener(HandleEyeButton);
            _magnetButton.onClick.AddListener(HandleMagnetButton);
            _winConditionChecker.OnWin += HandleOnWin;
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            _eyeButton.onClick.RemoveListener(HandleEyeButton);
            _magnetButton.onClick.RemoveListener(HandleMagnetButton);
            _winConditionChecker.OnWin -= HandleOnWin;
        }

        private void HandleOnWin() => gameObject.SetActive(false);


        private void HandleMagnetButton()
        {
            if (!_puzzleTray.CanDropPieces()) return;

            _puzzleTray.DropRandomPiecesAsync(_magnetPiecesCount, this.GetCancellationTokenOnDestroy());
            GlobalService.GameData.Data.Magnets -= 1;
            GlobalService.GameData.Save();
            UpdateMagnetButtonState();
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseMagnet, Amount = 1 });
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseBooster, Amount = 1 });
        }

        private void HandleEyeButton()
        {
            if (_fullImageHandler.IsActive()) return;

            _fullImageHandler.ShowFullImage();
            GlobalService.GameData.Data.Eye -= 1;
            GlobalService.GameData.Save();
            UpdateEyeButtonState();
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseEye, Amount = 1 });
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseBooster, Amount = 1 });
        }

        private void HandleLevelStart(LevelStartEvent @event)
        {
            gameObject.SetActive(true);
            UpdateEyeButtonState();
            UpdateMagnetButtonState();
        }

        private void UpdateEyeButtonState()
        {
            _eyeButton.interactable = GlobalService.GameData.Data.Eye > 0;
            _eyeCount.SetText($"{GlobalService.GameData.Data.Eye}");
        }

        private void UpdateMagnetButtonState()
        {
            _magnetButton.interactable = GlobalService.GameData.Data.Magnets > 0;
            _magnetCount.SetText($"{GlobalService.GameData.Data.Magnets}");
        }
    }
}