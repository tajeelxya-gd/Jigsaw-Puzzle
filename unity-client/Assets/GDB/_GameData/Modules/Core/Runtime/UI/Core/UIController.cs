using System;
using Client.Runtime;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameplayPanel _gamePlayPanel;
    public IProgress LevelProgressVisual => _gamePlayPanel.LevelProgressVisual;
    [SerializeField] private ScreenBase _winScreen;
    [SerializeField] private ScreenBase _loseScreen;
    [SerializeField] private SettingsManager _settingsManager;

    private IWinConditionChecker _winConditionChecker;

    public void Inject(IWinConditionChecker winConditionChecker)
    {
        _winConditionChecker = winConditionChecker;
        _winConditionChecker.OnAdvance += OnAdvance;
        _winScreen.Inject();
    }

    private void OnAdvance(float progress)
    {
        _gamePlayPanel.LevelProgressVisual.UpdateProgress(progress);
    }

    private void Start()
    {
        SignalBus.Subscribe<OnLevelCompleteSignal>(OnLevelCompleted);
        SignalBus.Subscribe<OnlevelFailSignal>(OnLevelFail);
    }

    public void Initialize(LevelData levelData)
    {
        _settingsManager.Inject(levelData);
        _gamePlayPanel.Initialize(levelData, _settingsManager);
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

        if (_winConditionChecker != null)
            _winConditionChecker.OnAdvance -= OnAdvance;

    }
}