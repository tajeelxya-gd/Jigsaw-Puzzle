using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    [SerializeField] private int _rewardAmount = 25;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject[] _racers;
    [SerializeField] private int _totalWinsToWin = 5;
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private AIManager _aiManager;
    [SerializeField] private GameObject _findOpponentPanel;
    [SerializeField] private GameObject _searchingPanel;
    [SerializeField] private GameObject _startRaceInfoPanel;
    [SerializeField] private GameObject _racePanel;
    [SerializeField] private GameObject _winFailPanel;
    [SerializeField] private Transform _startPoint;
    [SerializeField] private TMP_Text _raceTimerToCopyText;
    [SerializeField] private TMP_Text _raceTimerModule;

    private IGetWins[] _racersWins;
    private IGetCarType[] _carTypeGetter;
    private bool _isEventActive = false;
    private DataBaseService<EventData> _eventDataService;
    private EventData _eventData;
    private PanelScaling _panelScaling;
    private int _levelAtEventStart = 1;
    private int _currentLevel;
    private bool _raceStart = false;
    private bool _hasRaceEventEnded = false;

    [SerializeField] private Button _findOpponentButton;
    private WaitForSeconds _startWait = new WaitForSeconds(0.5f);
    private IEnumerator Start()
    {
        _findOpponentButton.onClick.RemoveAllListeners();
        _currentLevel = GlobalService.GameData.Data.LevelIndex;
        _panelScaling = _mainPanel.GetComponent<PanelScaling>();
        SignalBus.Subscribe<OnRaceEventEndSignal>(EndEvent);
        _eventDataService = new DataBaseService<EventData>();
        _eventData = _eventDataService.Load_Get();
        _findOpponentButton.onClick.AddListener(OpenSearchingPanel);

        if (_eventData == null)
        {
            _eventData = new EventData();
            _eventData.isEventActive = false;
            _eventData._levelAtEventStart = GlobalService.GameData.Data.LevelIndex;
            _eventData.raceStart = false;
        }

        _isEventActive = _eventData.isEventActive;
        ShowOpenRaceRequest();
        _levelAtEventStart = _eventData._levelAtEventStart;
        _raceStart = _eventData.raceStart;
        yield return _startWait;


        if (_isEventActive)
        {
            StartEvent();
            _racePanel.SetActive(true);
            _findOpponentPanel.SetActive(false);
            _raceTimerModule.SetText(_raceTimerToCopyText.text);
        }

        SignalBus.Subscribe<WinCheck>(CheckWinner);
        _racersWins = new IGetWins[_racers.Length];
        for (int i = 0; i < _racers.Length; i++)
        {
            _racersWins[i] = _racers[i].GetComponent<IGetWins>();
        }

        _carTypeGetter = new IGetCarType[_racers.Length];
        for (int i = 0; i < _racers.Length; i++)
        {
            _carTypeGetter[i] = _racers[i].GetComponent<IGetCarType>();
        }

        Debug.LogError("_levelAtEventStart:" + _levelAtEventStart);
        Debug.LogError("Current level:" + _currentLevel);
        if (!_isEventActive && _currentLevel > (int)OnBoardingConfig.OnBoardingType.TheGreatRace)
        {
            EndEvent(null);
            yield break;
        }
        int temp = _currentLevel - _levelAtEventStart;
        int _wonLevels = Mathf.Clamp(temp, 0, 5);
        for (int i = 0; i < _wonLevels; i++)
        {
            Debug.LogError("Signal Sent");
            SignalBus.Publish(new LevelWin());
            _levelAtEventStart = _currentLevel;
            _eventData._levelAtEventStart = _levelAtEventStart;
            _eventDataService.Save(_eventData);
        }
    }

    void ShowOpenRaceRequest()
    {
        GameData data = GlobalService.GameData;
        bool hasLevelReached = data.Data.LevelIndex > (int)OnBoardingConfig.OnBoardingType.TheGreatRace;
        if (GameStateGlobal.GreatRaceRequestedAlready) return;
        if (_isEventActive || !hasLevelReached) return;
        PopCommandExecutionResponder.AddCommand(new OnShowGreatRaceInfoCommand(PopCommandExecutionResponder.PopupPriority.Medium,
            execution =>
            {
                GameStateGlobal.GreatRaceRequestedAlready = true;
                _startRaceInfoPanel.gameObject.SetActive(true);
                DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.PanelPop); });

            }));
    }

    public void CloseRaceInfoPanel()
    {
        _startRaceInfoPanel.gameObject.SetActive(false);
        AudioController.PlaySFX(AudioType.PanelClose);
        PopCommandExecutionResponder.RemoveCommand<OnShowGreatRaceInfoCommand>();
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<WinCheck>(CheckWinner);
    }

    private void CheckWinner(WinCheck signal)
    {
        for (int i = 0; i < _racersWins.Length; i++)
        {
            int wins = _racersWins[i].GetWins();
            SignalBus.Publish(new AssignRanks());
            if (wins == _totalWinsToWin)
            {
                _aiManager.StopAutoAI();
                _isEventActive = false;
                _eventData.isEventActive = _isEventActive;
                _eventDataService.Save(_eventData);
                StartCoroutine(OpenWinFailPanelAfterDelay(2f));
            }
        }
    }

    private IEnumerator OpenWinFailPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _racePanel.SetActive(false);
        _winFailPanel.SetActive(true);
        OpenPanel();
    }

    [Button]
    public void StartEvent()
    {
        _isEventActive = true;
        _playerManager.CanAct = true;
        _aiManager.CanAct = true;
        _aiManager.StartAutoAI();
        _eventData.isEventActive = _isEventActive;
        _eventDataService.Save(_eventData);
        PublishOnMissionCompleteSignal();
        if (_raceStart) return;
        _raceStart = true;
        _levelAtEventStart = GlobalService.GameData.Data.LevelIndex;
        _eventData._levelAtEventStart = _levelAtEventStart;
        _eventData.raceStart = _raceStart;
        Debug.LogError("Level at start:" + _levelAtEventStart);
        _eventDataService.Save(_eventData);
        SignalBus.Publish(new OnRaceEventStartSignal());
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Events", nameof(AnalyticEventType.RaceEvent), "Start");

    }

    void PublishOnMissionCompleteSignal()
    {
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.JoinTinyRacer, Amount = 1 });
    }

    [Button]
    public void EndEvent(OnRaceEventEndSignal signal)
    {
        _raceStart = false;
        _isEventActive = false;
        _playerManager.CanAct = false;
        _aiManager.CanAct = false;
        _searchingPanel.SetActive(false);
        _racePanel.SetActive(false);
        _winFailPanel.SetActive(false);
        _mainPanel.SetActive(false);
        _findOpponentPanel.SetActive(true);
        _eventData.isEventActive = _isEventActive;
        _eventDataService.Save(_eventData);
        Reset();
    }

    public void OpenSearchingPanel()
    {
        Debug.LogError("ENTER RACE OH YEAH");
        _searchingPanel.SetActive(true);
        Reset();
        StartEvent();
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
    }

    public void OpenPanel()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _mainPanel.SetActive(true);
        _panelScaling.ScaleIn();
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Events", nameof(AnalyticEventType.RaceEvent), "Shown");


    }

    public void ClosePanel()
    {
        AudioController.PlaySFX(AudioType.PanelClose);
        HapticController.Vibrate(HapticType.Btn);
        _panelScaling.ScaleOut();
        StartCoroutine(CloseAfterDelay());
        CloseOnBoardingCommandIfYes();

    }

    void CloseOnBoardingCommandIfYes()
    {
        Debug.Log("CloseOnBoardingCommandIfYes :: " + PopCommandExecutionResponder.HasCommand<OnBoardingMenuCommand>());
        // if (PopCommandExecutionResponder.HasCommand<OnBoardingMenuCommand>())
        //  {
        PopCommandExecutionResponder.RemoveCommand<OnBoardingMenuCommand>();
        // }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        _mainPanel.SetActive(false);
        PopCommandExecutionResponder.RemoveCommand<OnShowGreatRaceInfoCommand>();
    }

    public void ClaimReward()
    {
        IBulkPopService _bulkPopService = GlobalService.IBulkPopService;
        _bulkPopService.PlayEffect(_rewardAmount, PopBulkService.BulkPopUpServiceType.Coins, _startPoint.position);
    }

    public void Reset()
    {
        _playerManager.Reset();
        _aiManager.Reset();
    }
}

public class AssignRanks : ISignal
{
}

public class WinCheck : ISignal
{
}

public class EventData
{
    public bool isEventActive;
    public bool raceStart;
    public int _levelAtEventStart;
}

public class AiWinsRace : ISignal
{
}
public class OnRaceEventStartSignal : ISignal
{
}