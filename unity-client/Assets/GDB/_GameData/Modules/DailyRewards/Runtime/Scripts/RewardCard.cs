using System;
using Coffee.UIExtensions;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UniTx.Runtime;

public class RewardCard : MonoBehaviour
{
    [SerializeField] private int _day;
    [SerializeField] private GameObject _collectedVisual;
    [SerializeField] private GameObject _dayGoneVisual;
    [SerializeField] private GameObject _currentDayIndicator;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private ObjectScaleEffect _scaleEffect;
    [SerializeField] private CanvasGroup _shineEffect;
    [SerializeField] private ParticleSystem _claimParticle;
    [SerializeField] private GameObject _bgShineEffect;
    [SerializeField] private UIParticle _particle;
    [SerializeField] private RewardContainerUI[] _rewardContainerUI;
    [SerializeField] private RectTransform _rewardContainerRectTransform;
    [SerializeField] private float _overrideScaleFactor = 1;
    [SerializeField] private UnityEvent _onClaimedReward;
    [ReadOnly, SerializeField]
    private int _currentDay; // Set by DailyRewardManager
    [ReadOnly, SerializeField]
    private bool _rewardCollected;
    private bool _pastDay = false;
    private const string RewardCollectedKey = "RewardCollected_Day";
    public bool IsCollected() => _rewardCollected;
    public bool IsPastDay() => _pastDay;

    private void Awake()
    {
        SignalBus.Subscribe<OnDailyRewardClaim>((_) => OnClaimRewardButtonClicked());
        SignalBus.Subscribe<OnShowDailyRewardPopUpEffect>(OnShowRewadPopBulk);
    }

    private void OnValidate()
    {
        _rewardCollected = PlayerPrefs.GetInt(RewardCollectedKey + _day, 0) == 1;
    }

    public void OnShowRewadPopBulk(OnShowDailyRewardPopUpEffect signal)
    {
        if (_day != _currentDay) return;
        DOVirtual.DelayedCall(1.25f, () => { OnClaimReward(_rewardData); });
    }

    private void OnEnable()
    {
        UpdateCollectedVisual();

        if (_day == _currentDay && !_rewardCollected)
        {
            _scaleEffect.SetLooping(true);
            JumpEffect();
        }
        else
        {
            _scaleEffect.SetLooping(false);
        }
    }

    public void DayGoneVisual(bool state)
    {
        _dayGoneVisual.SetActive(state);
    }


    [ReadOnly, SerializeField]
    private DailyRewardData _rewardData;

    void RemoveExistingContainerElements()
    {
        if (_rewardContainerRectTransform.childCount == 0) return;
        foreach (Transform child in _rewardContainerRectTransform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public void Initialize(DailyRewardData rewardData)
    {
        _rewardData = rewardData;
        if (_dayText != null)
            _dayText.text = rewardData._dayText;

        //RemoveExistingContainerElements();

        for (int i = 0; i < rewardData._progressModels.Length; i++)
        {
            RewardContainerUI _dataContainer = _rewardContainerUI[i];//Instantiate(_rewardContainerUI,_rewardContainerRectTransform);
            _dataContainer.transform.parent = _rewardContainerRectTransform;
            _dataContainer.transform.localScale = Vector3.one * _overrideScaleFactor;
            _dataContainer.Init(rewardData._progressModels[i]._rewardIcon, rewardData._progressModels[i].rewardChestAmount);
            _dataContainer.gameObject.SetActive(true);
        }
        _day = rewardData._currentDay;

        LoadCollectedState();
        UpdateCollectedVisual();
    }

    // Called by DailyRewardManager to update current day
    public void GetCurrentDay(DailyRewardTimerData signal)
    {
        _currentDay = signal._currentDay;
        UpdateCollectedVisual();
    }

    public void MarkAsCollected()
    {
        _scaleEffect.SetLooping(false);
        _scaleEffect.MoveDelayed(0.25f);

        _rewardCollected = true;
        SaveCollectedState();
        UpdateCollectedVisual();
    }

    public void OnClaimExplicit()
    {
        OnClaimReward(_rewardData);
    }

    void OnClaimRewardButtonClicked()
    {
        if (!_currentDayIndicator.gameObject.activeInHierarchy) return;
        _shineEffect.DOFade(0, 1).From(1);
        _claimParticle.Play();
        _scaleEffect.SetLooping(false);
        _scaleEffect.StopAnimation();
        _bgShineEffect.gameObject.SetActive(false);
        _onClaimedReward?.Invoke();
        AudioController.PlaySFX(AudioType.ChestOpen);
        SendAnalyticEvent();

    }

    void SendAnalyticEvent()
    {
        int currentWeekDay = GlobalService.GameData.Data.CurrentDailyRewardWeek;
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData,
            "Events", nameof(AnalyticEventType.DailyRewards),
            "Week", currentWeekDay.ToString(),
            "Day", _day.ToString());
        if (_day == 7)
        {
            ++GlobalService.GameData.Data.CurrentDailyRewardWeek;
            GlobalService.GameData.Save();
        }
        Debug.Log("Analytic Event Send :: Daily Reward");
    }
    void OnShowCommandDisabled()
    {
        PopCommandExecutionResponder.RemoveCommand<DailyRewardShowCommand>();
    }
    void OnClaimReward(DailyRewardData _currentRewardProgressData)
    {
        if (_currentRewardProgressData == null) return;
        GameData _gameData = GlobalService.GameData;
        IBulkPopService _bulkPopService = GlobalService.IBulkPopService;
        foreach (var _reward in _currentRewardProgressData._progressModels)
        {
            UniStatics.LogInfo("showing reward of type :: " + _reward.rewardType);
            switch (_reward.rewardType)
            {
                case WeeklyRewardType.None:
                    break;
                case WeeklyRewardType.Hammer:
                    //_gameData.Data.Hammer += _reward.rewardChestAmount;
                    _bulkPopService.PlayEffect(_reward.rewardChestAmount, PopBulkService.BulkPopUpServiceType.Hammer, transform.position, 3, OnShowCommandDisabled);
                    break;
                case WeeklyRewardType.InfiniteHealth:
                    _bulkPopService.PlayEffect(_reward.rewardChestAmount, PopBulkService.BulkPopUpServiceType.Health, transform.position, 10, OnShowCommandDisabled);
                    break;
                case WeeklyRewardType.Coin:
                    _bulkPopService.PlayEffect(_reward.rewardChestAmount, PopBulkService.BulkPopUpServiceType.Coins, transform.position, 10, OnShowCommandDisabled);
                    break;
                case WeeklyRewardType.PremiumCoin:
                    _gameData.Data.PremiumCoins += _reward.rewardChestAmount;
                    break;
                case WeeklyRewardType.MagicWand:
                    //_gameData.Data.Wand += _reward.rewardChestAmount;
                    _bulkPopService.PlayEffect(_reward.rewardChestAmount, PopBulkService.BulkPopUpServiceType.Wand, transform.position, 4, OnShowCommandDisabled);
                    break;
                case WeeklyRewardType.Magnet:
                    //_gameData.Data.Magnets += _reward.rewardChestAmount;
                    _bulkPopService.PlayEffect(_reward.rewardChestAmount, PopBulkService.BulkPopUpServiceType.Magnets, transform.position, 4, OnShowCommandDisabled);

                    break;
                case WeeklyRewardType.PopTreasureBox:
                    //_gameData.Data.SlotPopper += _reward.rewardChestAmount;
                    _bulkPopService.PlayEffect(_reward.rewardChestAmount, PopBulkService.BulkPopUpServiceType.SlotPopper, transform.position, 4, OnShowCommandDisabled);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public void UpdateCollectedVisual()
    {
        bool isPastDay = _day < _currentDay;
        bool isToday = _day == _currentDay;

        _dayGoneVisual.SetActive(false);
        _collectedVisual.SetActive(false);

        _pastDay = isPastDay;

        if (isToday)
        {
            if (_rewardCollected)
            {
                _dayGoneVisual.SetActive(true);
                _collectedVisual.SetActive(true);
                _particle.Stop();
                _particle.gameObject.SetActive(false);
            }
            else
            {
                _particle.gameObject.SetActive(true);
                _particle.Play();
            }
        }
        else if (isPastDay)
        {
            if (_rewardCollected)
            {
                _dayGoneVisual.SetActive(true);
                _collectedVisual.SetActive(true);
            }
            else
            {
                _particle.Stop();
                _particle.gameObject.SetActive(false);
                _dayGoneVisual.SetActive(true);
            }
        }
        else
        {
            // Future rewards
            _particle.Stop();
            _particle.gameObject.SetActive(false);
            _dayGoneVisual.SetActive(true);
        }
    }

    public void EnableCurrentDayIndicator()
    {
        _currentDayIndicator.SetActive(true);
    }

    public void DisableCurrentDayIndicator()
    {
        _currentDayIndicator.SetActive(false);
    }

    private void JumpEffect()
    {
        _scaleEffect.Move();
    }

    private void StopJumpEffect()
    {
        _scaleEffect.StopAnimation();
    }

    private void LoadCollectedState()
    {
        _rewardCollected = PlayerPrefs.GetInt(RewardCollectedKey + _day, 0) == 1;
    }

    private void SaveCollectedState()
    {
        UniStatics.LogInfo("reward collected :: for day : " + _day + _rewardCollected);
        PlayerPrefs.SetInt(RewardCollectedKey + _day, _rewardCollected ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ResetCollectedState()
    {
        _rewardCollected = false;
        SaveCollectedState();
        UpdateCollectedVisual();
        _dayGoneVisual.SetActive(false);
        StopJumpEffect();
        _particle.Stop();
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnDailyRewardClaim>((_) => OnClaimRewardButtonClicked());
        SignalBus.Unsubscribe<OnShowDailyRewardPopUpEffect>(OnShowRewadPopBulk);

    }
}
