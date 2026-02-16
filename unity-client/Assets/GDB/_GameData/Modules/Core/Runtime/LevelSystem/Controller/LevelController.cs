using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField, Range(1, 1000)] private int _totalLevelCount;

    [SerializeField] private bool _canTest = false;
    [ShowIf("@_canTest == true")][SerializeField, Range(1, 1000)] private int _testLevelNumber;

    [SerializeField] private GameController _gameController;

    [ReadOnly] public LevelData levelData;
    public void Initialize()
    {
        if (_canTest)
            LoadLevel(_testLevelNumber);
        else
            LoadLevel(GlobalService.GameData.Data.LevelNumber);

        SignalBus.Subscribe<OnLevelCompleteSignal>(OnLevelComplete);
        SignalBus.Subscribe<OnlevelFailSignal>(OnLevelFail);
    }

    public int GetCurrentLevel()
    {
        return _canTest ? _testLevelNumber : GlobalService.GameData.Data.LevelNumber;
    }
    private void LoadLevel(int id)
    {
        int wrappedId = GetWrappedLevel(id);

        TextAsset json = Resources.Load<TextAsset>($"Levels/Level {wrappedId}");
        LevelData temp = ScriptableObject.CreateInstance<LevelData>();
        JsonUtility.FromJsonOverwrite(json.text, temp);
        levelData = temp;

        DOVirtual.DelayedCall(Time.deltaTime, () => { OnLevelLoaded(wrappedId); });
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + id, "Start");
    }

    void OnLevelLoaded(int id) => SignalBus.Publish(new OnLevelLoadedSignal { levelNo = id });
    private void OnLevelComplete(ISignal signal)
    {
        GlobalService.GameData.Data.LevelNumber += 1;
        GlobalService.GameData.Save();
        Time.timeScale = 1;
        PublishMissionObjectiveSignals();
        Time.timeScale = 1;
    }

    void PublishMissionObjectiveSignals()
    {
        if (levelData != null && levelData.levelType == LevelType.Hard)
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.WinHardLevel, Amount = 1 });
        if (levelData != null && levelData.levelType == LevelType.SuperHard)
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.WinSuperHardLevel, Amount = 1 });

        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.WinLevel, Amount = 1 });
        //Debug.LogError("Published WinLevel mission objective signal");
    }

    private void OnLevelFail(ISignal signal)
    {
        //PoolManager.ReturnAllItems();
    }
    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnLevelCompleteSignal>(OnLevelComplete);
        SignalBus.Unsubscribe<OnlevelFailSignal>(OnLevelFail);
    }

    const int FIRST_REPEAT_LEVEL = 101;
    int GetWrappedLevel(int level)
    {
        if (level <= _totalLevelCount)
            return level;

        int repeatRange = _totalLevelCount - FIRST_REPEAT_LEVEL + 1;
        return FIRST_REPEAT_LEVEL + ((level - _totalLevelCount - 1) % repeatRange);
    }
}