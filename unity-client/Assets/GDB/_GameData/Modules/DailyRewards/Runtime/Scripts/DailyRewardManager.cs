using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    [SerializeField] private int _presetIndex = 0;
    [SerializeField] private GameObject _dailyRewardPanel;
    [SerializeField] private GameObject _claimButton;
    [SerializeField] private GameObject _claimX2Button;
    [SerializeField] private GameObject _infoText;
    [SerializeField] private DailyRewardPresets _dailyRewardPresets;
    [SerializeField] private RewardCard[] _rewardCards;
    [ReadOnly, SerializeField]
    private int _currentDay = 1;
    private PanelScaling _rewardPanelScaling;
    DailyRewardTimer _dailyRewardTimer;

    public void Inject(DailyRewardTimer dailyRewardTimer)
    {
        _dailyRewardTimer = dailyRewardTimer;
        if (_presetIndex >= _dailyRewardPresets._dailyRewardPresetsData.Length)
        {
            _presetIndex = 0;
        }

        ApplyPreset(_presetIndex);
        Initialize();
    }


    private void Start()
    {

    }

    void Initialize()
    {
        SignalBus.Subscribe<StreakBreakSignal>(OnResetStreakSignal);
        _rewardPanelScaling = _dailyRewardPanel.GetComponent<PanelScaling>();
        _currentDay = _dailyRewardTimer.GetActiveRewardCollectionDay();
        UpdateClaimButtons(_currentDay);
        UpdateCurrentDayIndicator();
        //CheckForMissedDays();
        bool rewardAvailable = IsRewardAvailableToday();
        bool missedReward = HasMissedReward();
        if (rewardAvailable)
        {
            OpenDailyRewardPanel();
        }

        if (missedReward)
        {
            SignalBus.Publish(new RewardMissSignal());
        }
    }
    Sequence onStreakResetSequence;
    void OnResetStreakSignal(StreakBreakSignal signal)
    {
        // Kill any previous sequence to prevent overlaps
        onStreakResetSequence?.Kill();
        AudioController.PlaySFX(AudioType.Loss);
        onStreakResetSequence = DOTween.Sequence();
        _claimButton.gameObject.SetActive(false);

        float scaleDuration = 0.2f; // Speed of the scale
        float staggerDelay = 0.02f;    // Delay between each card starting
        float waitTime = 0.2f;       // Time to stay at zero scale

        // 1. Scale down all cards with a stagger
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            onStreakResetSequence.Join(_rewardCards[i].transform.DOScale(Vector3.zero, scaleDuration)
                .SetDelay(i * staggerDelay)
                .SetEase(Ease.InBack));
        }

        // 2. Wait for a moment
        onStreakResetSequence.AppendInterval(waitTime);

        // 3. Scale them back up to 1
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            onStreakResetSequence.Join(_rewardCards[i].transform.DOScale(Vector3.one, scaleDuration)
                .SetDelay(i * staggerDelay)
                .SetEase(Ease.OutBack));
        }


        onStreakResetSequence.AppendCallback(() =>
        {
            _claimButton.gameObject.SetActive(true);
        });
    }

    public void OpenDailyRewardPanel()
    {
        PopCommandExecutionResponder.AddCommand(
            new DailyRewardShowCommand(
                PopCommandExecutionResponder.PopupPriority.Critical,
                executionAction => ShowDailyRewardPanel()
            )
        );
    }

    void ShowDailyRewardPanel()
    {
        _dailyRewardPanel.SetActive(true);
        if (_rewardPanelScaling)
            _rewardPanelScaling.ScaleIn();

        Debug.LogError("SOUND SHOULD HAVE PLAYED");
        DOVirtual.DelayedCall(0.3f, () => AudioController.PlaySFX(AudioType.DailyRewards));
        DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.PanelPop); });

    }

    private bool IsRewardAvailableToday()
    {
        int index = _currentDay - 1;
        if (index < 0 || index >= _rewardCards.Length) return false;
        return !_rewardCards[index].IsCollected();
    }

    private bool HasMissedReward()
    {
        return _dailyRewardTimer.GetDaysPassedSinceLastClaims() > 1;
        // for (int i = 0; i < _rewardCards.Length; i++)
        // {
        //     if (_rewardCards[i].IsPastDay() && !_rewardCards[i].IsCollected())
        //     {
        //         return true;
        //     }
        // }
        //
        // return false;
    }

    private void UpdateCurrentDayIndicator()
    {
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            _rewardCards[i].DisableCurrentDayIndicator();
            _rewardCards[i].GetCurrentDay(new DailyRewardTimerData { _currentDay = _currentDay });
            _rewardCards[i].UpdateCollectedVisual();
            //_rewardCards[i].DayGoneVisual(i < _currentDay - 1);
        }

        int index = _currentDay - 1;
        if (index >= 0 && index < _rewardCards.Length)
        {
            _rewardCards[index].EnableCurrentDayIndicator();
            //_rewardCards[index].DayGoneVisual(false);
        }
    }

    public void GoToNextDay(int newDay)
    {
        _currentDay = newDay;
        UpdateCurrentDayIndicator();
        // if (_currentDay == 1)
        // {

        // }

        UpdateClaimButtons(_currentDay);
    }



    private void UpdateClaimButtons(int currentDay)
    {
        int index = currentDay - 1;
        if (index < 0 || index >= _rewardCards.Length)
        {
            _claimButton.SetActive(false);
            //_claimX2Button.SetActive(false);
            return;
        }

        bool canClaim = !_rewardCards[index].IsCollected();
        _claimButton.SetActive(canClaim);
        //_claimX2Button.SetActive(canClaim);
        //_infoText.SetActive(!canClaim);
    }

    public void OnClaimButtonClicked()
    {
        int index = _currentDay - 1;
        if (index < 0 || index >= _rewardCards.Length) return;
        var rewardCard = _rewardCards[index];
        if (rewardCard.IsCollected()) return;
        rewardCard.MarkAsCollected();
        _claimButton.SetActive(false);
        //_claimX2Button.SetActive(false);
        //_infoText.SetActive(true);
        SignalBus.Publish(new OnDailyRewardClaim());
        Debug.LogError($"Claimed Day {_currentDay} reward (x1)");
        AudioController.PlaySFX(AudioType.CollectSoft);
        StartCoroutine(ClosePanel());
    }

    private IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(1.5f);
        if (_rewardPanelScaling)
            _rewardPanelScaling.ScaleOut();
        _dailyRewardPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.3f).From(1)
            .OnComplete(() => { _dailyRewardPanel.SetActive(false); });

    }

    private void ResetAllRewards()
    {
        foreach (var card in _rewardCards)
        {
            card.ResetCollectedState();
        }
    }

    private void ApplyPreset(int presetIndex)
    {
        var preset = _dailyRewardPresets._dailyRewardPresetsData[presetIndex];
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            if (i < preset._dailyRewards.Length)
            {
                var data = preset._dailyRewards[i];
                Debug.LogError("Daily Reward: " + data._dayText);
                data._currentDay = i + 1;
                _rewardCards[i].Initialize(data);
            }
        }
    }
    public void ResetEverything()
    {
        ResetAllRewards();
        _presetIndex++;
        if (_presetIndex >= _dailyRewardPresets._dailyRewardPresetsData.Length)
        {
            _presetIndex = 0;
        }

        ApplyPreset(_presetIndex);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<StreakBreakSignal>(OnResetStreakSignal);
    }
}

public class OnDailyRewardClaim : ISignal
{
    public bool IsStreakCompleted = false;
}

public class OnShowDailyRewardPopUpEffect : ISignal { }


// public void OnClaimButtonClickedX2()
// {
//     int index = _currentDay - 1;
//     if (index < 0 || index >= _rewardCards.Length) return;
//     var rewardCard = _rewardCards[index];
//     if (rewardCard.IsCollected()) return;
//     var rewards = rewardCard.GetRewards();
//     for (int i = 0; i < rewards.Length; i++)
//     {
//         // GIVE REWARD X2
//     }
//     rewardCard.MarkAsCollected();
//     _claimButton.SetActive(false);
//     //_claimX2Button.SetActive(false);
//     //_infoText.SetActive(true);
//     Debug.LogError($"Claimed Day {_currentDay} reward (x2)");
// }