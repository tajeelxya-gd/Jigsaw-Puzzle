using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class InAppBoughtSuccessBoughtMessage : MonoBehaviour
{
    [SerializeField] private GameObject _successPanel;
    [SerializeField] private GameObject _failedPanel;
    void Start()
    {
        SignalBus.Subscribe<OnInAppSuccessFullyBought>(OnInAppBoughtSuccessMessage);
    }


    void OnInAppBoughtSuccessMessage(OnInAppSuccessFullyBought signal)
    {
        Debug.LogError("showing inapp message");
        GameObject panelToShow = signal.IsSuccess ? _successPanel.gameObject : _failedPanel.gameObject;
        DOVirtual.DelayedCall(1.5f, () => {
            if (panelToShow != null) 
                panelToShow.SetActive(true);
        }).SetTarget(this); 
        AudioController.PlaySFX(signal.IsSuccess ? AudioType.RewardPopUp : AudioType.PanelPop);
    }

    [Button]
    void ShowPanelDebug(bool success)
    {
        _successPanel.gameObject.SetActive(success);
        _failedPanel.gameObject.SetActive(!success);
    }
    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnInAppSuccessFullyBought>(OnInAppBoughtSuccessMessage);
        
    }
}

public class OnInAppSuccessFullyBought : ISignal
{
    public bool IsSuccess = true;
}
