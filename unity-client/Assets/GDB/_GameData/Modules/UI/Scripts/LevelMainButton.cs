using UnityEngine;

public class LevelMainButton : MonoBehaviour
{
    private GameData _gameData;
    ITimeService timeService_freeTimer;
    private LevelData _leveldata;

    public void Inject(LevelData leveldata)
    {
        _leveldata = leveldata;
        _gameData = GlobalService.GameData;
        timeService_freeTimer =
            new RealTimeService(PlayerHealthTimerType.InfiniteHealthTimer.ToString(), OnInfiniteHealth);
    }

    public void LoadLevel()
    {
        if (_gameData == null || _gameData.Data == null || timeService_freeTimer == null) return;

        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);

        if (_gameData.Data.AvailableLives == 0 && !timeService_freeTimer.IsRunning())
        {
            SignalBus.Publish(new OnNoMoreLivesSignal());
            return;
        }
        SignalBus.Publish(new OnSceneShiftSignal { SceneName = "GamePlay", DoFakeLoad = false, levelType = _leveldata.levelType });
    }
    void OnInfiniteHealth() { }

    private void AddInfiniteLivesTime(OnHealthUpdateSignal signal)
    {
        if (!timeService_freeTimer.IsRunning())
            timeService_freeTimer.StartTimer(signal.TimeToAdd * 60);
        else
            timeService_freeTimer.ExtendTimer(signal.TimeToAdd);
    }

    private void Start()
    {
        SignalBus.Subscribe<OnHealthUpdateSignal>(AddInfiniteLivesTime);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnHealthUpdateSignal>(AddInfiniteLivesTime);
    }
}