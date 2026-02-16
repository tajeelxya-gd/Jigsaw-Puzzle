using System;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardMiss : MonoBehaviour
{
    [SerializeField] private RewardCard[] _rewardCards;
    [SerializeField] private GameObject _missingDaysPanel;
    [SerializeField] private int _defaultDay = 1;
    private DailyRewardManager _dailyRewardManager;
    [SerializeField] private Button _reviveButton;
    [SerializeField] private Button _loseStreakButton;
    [SerializeField] private DailyRewardTimer _dailyRewardTimer;
    private void Awake()
    {
        SignalBus.Subscribe<RewardMissSignal>(OpenMissingDaysPanel);
    }

    private void Start()
    {
        _reviveButton.onClick.AddListener(GiveRewardForMissingDays);
        _loseStreakButton.onClick.AddListener(Reset);
    }

    public void Inject(DailyRewardManager dailyRewardManager)
    {
        _dailyRewardManager= dailyRewardManager;
    }

    private void OpenMissingDaysPanel(RewardMissSignal signal)
    {
        AudioController.PlaySFX(AudioType.PanelPop);
        _missingDaysPanel.SetActive(true);
        CheckCoins();
    }

    private void CheckCoins()
    {
        if(GlobalService.GameData.Data.Coins>=100)
        {
            _reviveButton.interactable = true;
        }
        else
        {
            _reviveButton.interactable = false;
        }
    }
    public void GiveRewardForMissingDays()
    {
        GlobalService.GameData.Data.Coins -= 100;
        GlobalService.GameData.Save();
        AudioController.PlaySFX(AudioType.PanelClose);
        
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            if (_rewardCards[i].IsPastDay() && !_rewardCards[i].IsCollected())
            {
                //GIVE REWARD TO PLAYER
                _rewardCards[i].MarkAsCollected();
                _rewardCards[i].OnClaimExplicit();
               // Debug.LogError("Reward Given for missed day: " + (i + 1));
            }
        }
    }

    public void Reset()
    {
        _dailyRewardTimer.ResetData();
        SignalBus.Publish(new DailyRewardTimerData()
        {
            _currentDay=_defaultDay
        });
        PlayerPrefs.SetInt("DailyRewardCurrentDay",1);
        PlayerPrefs.Save();
        foreach (var card in _rewardCards)
        {
            card.ResetCollectedState();
        }
        _dailyRewardManager.GoToNextDay(_defaultDay);
        SignalBus.Publish(new StreakBreakSignal());
        AudioController.PlaySFX(AudioType.PanelClose);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<RewardMissSignal>(OpenMissingDaysPanel);
    }
}

public class RewardMissSignal : ISignal
{
    
}

