using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinBundlePanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _closeButton, _crossBtn;
    [SerializeField] private CanvasGroup _canvasGroup;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _closeButton.onClick.AddListener(ClosePanel);
        _crossBtn.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<OnCoinBundleCalledSignal>(OpenPanel);
        SignalBus.Subscribe<OnInAppBuySignal>(CloseAfterInAppPurchase);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnCoinBundleCalledSignal>(OpenPanel);
        SignalBus.Unsubscribe<OnInAppBuySignal>(CloseAfterInAppPurchase);
    }

    private Action OnClose;
    private void OpenPanel(OnCoinBundleCalledSignal signal)
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _canvasGroup.alpha = 0.5f;
        _root.SetActive(true);
        _canvasGroup.DOFade(1, 0.25f).SetUpdate(true);
        OnClose = signal.OnClose;
        SignalBus.Publish(new InputRestrictSignal { restrict = true });
        Time.timeScale = 0f;
    }

    private void ClosePanel()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        SignalBus.Publish(new InputRestrictSignal { restrict = false });
        _root.SetActive(false);
        OnClose?.Invoke();
        OnClose = null;
        Time.timeScale = 1f;
        // _root.transform.GetChild(1).DOScale(0.4f, 0.2f).SetEase(Ease.OutQuad);
        // _root.transform.GetChild(0).DOScale(0.4f,0.2f).SetEase(Ease.OutQuad)
        //     .OnComplete(() => _root.SetActive(false));
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveListener(ClosePanel);
        _crossBtn.onClick.RemoveListener(ClosePanel);
    }
    
    private void CloseAfterInAppPurchase(OnInAppBuySignal signal)
    {
        ClosePanel();
    }
}

public class OnCoinBundleCalledSignal : ISignal
{
    public Action OnClose;
}