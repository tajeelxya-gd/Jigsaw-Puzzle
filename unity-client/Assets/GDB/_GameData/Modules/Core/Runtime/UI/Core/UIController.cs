using System;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameplayPanel _gamePlayPanel;
    public IProgress LevelProgressVisual => _gamePlayPanel.LevelProgressVisual;
    [SerializeField] private PowerupVisualController _powerupVisualController;
    [SerializeField] private ScreenBase _winScreen;
    [SerializeField] private ScreenBase _loseScreen;
    [SerializeField] private PowerUpTutorialPanel _powerUpTutorialPanel;
    [SerializeField] private PowerUpAdditionPanel _powerUpAdditionPanel;
    [SerializeField] private SettingsManager _settingsManager;

    private void Start()
    {
        SignalBus.Subscribe<OnLevelCompleteSignal>(OnLevelCompleted);
        SignalBus.Subscribe<OnlevelFailSignal>(OnLevelFail);
    }

    public void Initialize(LevelData levelData, CannonController cannonController, SpaceController spaceController)
    {
        _powerUpTutorialPanel.Initialize();
        _gamePlayPanel.Initialize(levelData, _settingsManager, cannonController, spaceController);
        _powerupVisualController.Initialize();
        _powerUpAdditionPanel.Initialize();
    }

    void OnLevelCompleted(OnLevelCompleteSignal signal)
    {
        _winScreen.ShowScreen<LevelType>(signal.levelType);
    }

    void OnLevelFail(OnlevelFailSignal signal)
    {
        //Debug.LogError("LEVEL FAILED");
        _loseScreen.ShowScreen<LevelFailType>(signal.levelFailType);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnLevelCompleteSignal>(OnLevelCompleted);
        SignalBus.Unsubscribe<OnlevelFailSignal>(OnLevelFail);
    }
}