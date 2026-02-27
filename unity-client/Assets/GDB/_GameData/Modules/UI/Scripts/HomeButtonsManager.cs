using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HomeButtonsManager : MonoBehaviour
{
    [SerializeField] private Button _homeButton, _storeButton, _goldMedalButton;
    [SerializeField] private PanelNavigator _page;
    [SerializeField] private ScrollRectArraySnapper _scrollSnapper;
    [SerializeField] private float _snapDuration = 0.5f;
    void Start()
    {
        SignalBus.Subscribe<OpenStoreSignal>(OpenStoreSignal);
        SignalBus.Subscribe<OnInAppBuySignal>(OnInappBuySignal);
    }
    public void OpenHome()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _page.GoToPage(1);
        _homeButton.interactable = false;
        _storeButton.interactable = true;
        _goldMedalButton.interactable = true;
    }

    void OnInappBuySignal(OnInAppBuySignal signal)
    {
        _homeButton.onClick.Invoke();
    }

    public void OpenStore()
    {
        //AudioController.PlaySFX(AudioType.ButtonClick);
        Store();
        SnapToTarget(0);
    }

    public void OpenStoreForHearts()
    {
        Store();
        SnapToTarget(4);
    }

    public void OpenStoreForCoins()
    {
        Store();
        SnapToTarget(5);
    }
    private void OpenStoreSignal(OpenStoreSignal signal)
    {
        OpenStoreForCoins();
    }
    public void OpenGoldMedal()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _page.GoToPage(2);
        _homeButton.interactable = true;
        _storeButton.interactable = true;
        _goldMedalButton.interactable = false;
    }

    private void Store()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _page.GoToPage(0);
        _homeButton.interactable = true;
        _storeButton.interactable = false;
        _goldMedalButton.interactable = true;
    }

    public void SnapToTarget(int index)
    {
        _scrollSnapper.SnapToItem(index, _snapDuration);
    }
    void OnDisable()
    {
        SignalBus.Unsubscribe<OpenStoreSignal>(OpenStoreSignal);
        SignalBus.Unsubscribe<OnInAppBuySignal>(OnInappBuySignal);

    }
}

public class OpenStoreSignal : ISignal
{ }