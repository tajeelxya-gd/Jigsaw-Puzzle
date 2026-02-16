using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AreYouSurePanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Image[] _hearts;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _timer;
    private Tween _blinkTween;
    ITimeService timeService_freeTimer;
    ITimeService timeService_resetHealthTimer;
    private bool _isInfinite = false;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        timeService_freeTimer =
            new RealTimeService(PlayerHealthTimerType.InfiniteHealthTimer.ToString(), OnInfiniteHealth);
        RemoveHeartAnimation();
        timeService_resetHealthTimer =
            new RealTimeService(PlayerHealthTimerType.ResetHealthTimer.ToString(), OnTimerEndedResetHealth);
        _continueButton.onClick.AddListener(OnContinueButtonClick);
        _isInfinite = timeService_freeTimer.IsRunning();
        SignalBus.Subscribe<AreYouSurePanelSignal>(OpenPanel);
    }
    void Update()
    {
        if (timeService_resetHealthTimer.IsRunning())
        {
            _timer.text = timeService_resetHealthTimer.GetFormattedTime();
        }
        else
        {
            _timer.text = "25M";
        }
    }

    private void OpenPanel(AreYouSurePanelSignal signal)
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        timeService_resetHealthTimer =
            new RealTimeService(PlayerHealthTimerType.ResetHealthTimer.ToString(), OnTimerEndedResetHealth);
        _root.SetActive(true);
        HeartAnimation();
        RemoveHeartAnimation();
        SignalBus.Publish(new InputRestrictSignal { restrict = true });
    }

    public void ClosePanel()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _root.SetActive(false);
        
        Time.timeScale = 1;
        SignalBus.Publish(new InputRestrictSignal { restrict = false });
    }

    private void OnContinueButtonClick()
    {
        if (!_isInfinite)
        {
            GlobalService.GameData.Data.AvailableLives = Math.Max(0, GlobalService.GameData.Data.AvailableLives - 1);
            GlobalService.GameData.Save();
            RemoveHeartAnimation();
            DOVirtual.DelayedCall(0.1f, () =>
                {
                    ClosePanel();
                    Time.timeScale = 1;
                    SignalBus.Publish(new OnSceneShiftSignal
                        { DoFakeLoad = true, FakeLoadTime = 2,OnFakeLoadCompleteEven = GoToHome});
                }
            ).SetUpdate(true);
        }
        else
        {
            ClosePanel();
            SignalBus.Publish(new OnSceneShiftSignal
                { DoFakeLoad = true, FakeLoadTime = 2,OnFakeLoadCompleteEven = GoToHome});
        }
    }
    
    private void GoToHome()
    {
        SceneManager.LoadScene(1);
    }

    private void HeartAnimation()
    {
        _blinkTween?.Kill();
        int availableLives = GlobalService.GameData.Data.AvailableLives;
        int blinkIndex = availableLives - 1;
        if (blinkIndex >= 0 && blinkIndex < _hearts.Length)
        {
            Image heart = _hearts[blinkIndex];
            _blinkTween = heart.DOFade(0.2f, 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine).SetUpdate(true).SetUpdate(true);
        }
    }

    private void RemoveHeartAnimation()
    {
        int availableLives = GlobalService.GameData.Data.AvailableLives;
        for (int i = 0; i < _hearts.Length; i++)
        {
            if (i < availableLives)
            {
                _hearts[i].gameObject.SetActive(true);
                _hearts[i].color = Color.white;
            }
            else
            {
                _hearts[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<AreYouSurePanelSignal>(OpenPanel);
    }

    void OnInfiniteHealth() { }
    void OnTimerEndedResetHealth() { }
}

public class AreYouSurePanelSignal : ISignal
{
}