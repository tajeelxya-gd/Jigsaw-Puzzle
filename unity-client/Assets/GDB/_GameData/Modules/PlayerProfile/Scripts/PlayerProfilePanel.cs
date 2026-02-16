using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfilePanel : MonoBehaviour
{
    [SerializeField] private PanelScaling _scaling;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        _closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        SignalBus.Publish(new PlayerProfilePanelOpenSignal());
        _scaling.ScaleIn();
        DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.PanelPop);});
        
    }

    void Reset()
    {
        transform.localScale = Vector3.one;
        _canvasGroup.alpha = 1;
    }
    public void ClosePanel()
    {
        SignalBus.Publish(new PlayerProfilePanelOpenSignal());
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        transform.DOScale(Vector3.one * 1.2f, 0.5f);
        _canvasGroup.DOFade(0, 0.25f).OnComplete(() => { gameObject.SetActive(false); Reset(); });
    }
}

public class PlayerProfilePanelOpenSignal : ISignal
{
}