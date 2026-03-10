using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NoMoreLives : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _rewardedAdButton;
    [SerializeField] private Button _refillByCoinsButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _refillText;
    [SerializeField] private int _heartsRefillCost = 125;
    ITimeService timeService_freeTimer;
    ITimeService timeService_resetHealthTimer;
    private bool _isInfiniteTimerActive = false;
    [SerializeField] private bool _gameplay = true;
    private GameData _gameData;

    private void Start()
    {
        _gameData = GlobalService.GameData;
        timeService_freeTimer = new RealTimeService(PlayerHealthTimerType.InfiniteHealthTimer.ToString(), OnTimerEnded);
        timeService_resetHealthTimer =
            new RealTimeService(PlayerHealthTimerType.ResetHealthTimer.ToString(), OnTimerEndedResetHealth);
        _isInfiniteTimerActive = timeService_freeTimer.IsRunning();
        RegisterButtonCallbacks();
        _refillText.text = _heartsRefillCost.ToString();
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<OnNoMoreLivesSignal>(OpenPanel);
        SignalBus.Subscribe<OnHealthUpdateSignal>(OnHealthUpdate);
        SignalBus.Subscribe<OnInAppBuySignal>(OnInAppPurchase);
    }

    void OnHealthUpdate(OnHealthUpdateSignal signal)
    {
        timeService_resetHealthTimer.ExtendTimer(signal.TimeToAdd);
    }

    void OnInAppPurchase(OnInAppBuySignal signal)
    {
        ClosePanel();
    }

    private void Update()
    {
        if (timeService_resetHealthTimer.IsRunning())
        {
            _timerText.text = timeService_resetHealthTimer.GetFormattedTime();
        }
    }

    private void RegisterButtonCallbacks()
    {
        _rewardedAdButton.onClick.AddListener(RewardedAd);
        _refillByCoinsButton.onClick.AddListener(HeartsRefillByCoins);
        _closeButton.onClick.AddListener(CrossButtonFunctionality);
    }

    [Button]
    private void OpenPanel(OnNoMoreLivesSignal signal)
    {
        if (this == null) return;
        if (_isInfiniteTimerActive) return;
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);

        _root.SetActive(true);
        // Reset scale and animate in
        var bg = _root.transform.GetChild(0);
        bg.DOKill();
        bg.localScale = Vector3.one * 0.4f;
        bg.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);

        SignalBus.Publish(new InputRestrictSignal { restrict = true });
    }

    [Button]
    private void ClosePanel()
    {
        if (this == null) return;
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        var bg = _root.transform.GetChild(0);
        bg.DOKill();
        bg.DOScale(0.4f, 0.25f).SetEase(Ease.InQuad).OnComplete(() => _root.SetActive(false)).SetUpdate(true);
        SignalBus.Publish(new InputRestrictSignal { restrict = false });
    }

    void CrossButtonFunctionality()
    {
        ClosePanel();

        if (_gameplay)
        {
            //Debug.LogError("COUNTRY ROAD TAKE ME HOME");
            SignalBus.Publish(new OnSceneShiftSignal
            { DoFakeLoad = true, FakeLoadTime = 2, OnFakeLoadCompleteEven = TakeToHome });
        }
    }

    void TakeToHome()
    {
        SceneManager.LoadScene(1);
    }

    private void HeartsRefillByCoins()
    {
        if (_gameData.Data.Coins >= _heartsRefillCost)
        {
            _gameData.Data.Coins -= _heartsRefillCost;
            _gameData.Data.AvailableLives = 5;
            _gameData.Save();
            ClosePanel();
            SignalBus.Publish(new CloseFailPanelSignal());
        }
        else
        {
            if (_gameplay)
            {
                SignalBus.Publish(new OnCoinBundleCalledSignal
                {
                    OnClose = () => { OpenPanel(new OnNoMoreLivesSignal()); }
                });
            }
            else
            {
                SignalBus.Publish(new OpenStoreSignal());
            }

            ClosePanel();
        }
    }

    private void RewardedAd()
    {
        // First Rewarded Ad then give one life
        // AdsManager.ShowRewarded("FreeLife " + GlobalService.GameData.Data.LevelIndex, () =>
        // {
        _gameData.Data.AvailableLives += 1;
        _gameData.Save();
        ClosePanel();
        SignalBus.Publish(new OnFailedSignal());
        // });
    }

    void OnTimerEnded()
    {
    }

    void OnTimerEndedResetHealth()
    {
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnNoMoreLivesSignal>(OpenPanel);
        SignalBus.Unsubscribe<OnHealthUpdateSignal>(OnHealthUpdate);
        SignalBus.Unsubscribe<OnInAppBuySignal>(OnInAppPurchase);


    }
}

public class OnNoMoreLivesSignal : ISignal
{
}

public class CloseFailPanelSignal : ISignal
{
}