using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class RaceEventUnlock : MonoBehaviour
{
    [SerializeField] private GameObject _lockedRaceEvent;
    [SerializeField] private GameObject _unlockedRaceEvent;
    [SerializeField] private Button _raceEventButton;
    public bool _hasEventBeenUnlocked = false;
    private RaceEventData _raceEventData;
    private DataBaseService<RaceEventData> _raceEventDataService;
    [Title("OnBoarding")]
    [SerializeField] private OnBoardingConfig.OnBoardingType _onBoardingType;
    GameData _gameData;

    private void Start()
    {
        _gameData = GlobalService.GameData;
        _raceEventDataService = new DataBaseService<RaceEventData>();
        _raceEventData = _raceEventDataService.Load_Get();
        if (_raceEventData == null)
        {
            _raceEventData = new RaceEventData();
            _raceEventData.isRaceEventUnlocked = false;
            _raceEventDataService.Save(_raceEventData);
        }
        SignalBus.Subscribe<RaceEventUnlockSignal>(UnlockRaceEvent);
        Test();
        _hasEventBeenUnlocked = _raceEventData.isRaceEventUnlocked;
        UpdateUI();
        DOVirtual.DelayedCall(Time.deltaTime * 10, LookForOnBoardingPanel);
    }
    void UpdateUI()
    {
        _lockedRaceEvent.gameObject.SetActive(_gameData.Data.LevelNumber < (int)_onBoardingType);
        _unlockedRaceEvent.gameObject.SetActive(_gameData.Data.LevelNumber >= (int)_onBoardingType);
    }

    void LookForOnBoardingPanel()
    {
        if (_unlockedRaceEvent.activeInHierarchy) SendMessage("InitOnBoarding");
    }
    private void UnlockRaceEvent(RaceEventUnlockSignal signal)
    {
        UpdateUI();
    }


    private void OnDestroy()
    {
        SignalBus.Unsubscribe<RaceEventUnlockSignal>(UnlockRaceEvent);
    }

    [Button]
    public void Test()
    {
        SignalBus.Publish(new RaceEventUnlockSignal());
    }
}

public class RaceEventUnlockSignal : ISignal
{
}
public class RaceEventData
{
    public bool isRaceEventUnlocked;
}