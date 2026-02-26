using System;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class BoosterPanel : MonoBehaviour, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Button _eyeButton;
        [SerializeField] private Button _magnetButton;

        private IFullImageHandler _fullImageHandler;
        private IPuzzleTray _puzzleTray;

        public void Inject(IResolver resolver)
        {
            _fullImageHandler = resolver.Resolve<IFullImageHandler>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public void Initialise()
        {
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            _eyeButton.onClick.AddListener(HandleEyeButton);
            _magnetButton.onClick.AddListener(HandleMagnetButton);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            _eyeButton.onClick.RemoveListener(HandleEyeButton);
            _magnetButton.onClick.RemoveListener(HandleMagnetButton);
        }

        private void HandleMagnetButton()
        {
        }

        private void HandleEyeButton()
        {
            if (_fullImageHandler.IsActive()) return;

            _fullImageHandler.ShowFullImage();
            GlobalService.GameData.Data.Eye -= 1;
            UpdateEyeButtonState();
        }

        private void HandleLevelStart(LevelStartEvent @event)
        {
            UpdateEyeButtonState();
            UpdateMagnetButtonState();
        }

        private void UpdateEyeButtonState()
        {
            _eyeButton.interactable = GlobalService.GameData.Data.Eye > 0;
        }

        private void UpdateMagnetButtonState()
        {
            _magnetButton.interactable = GlobalService.GameData.Data.Magnets > 0;
        }
    }
}