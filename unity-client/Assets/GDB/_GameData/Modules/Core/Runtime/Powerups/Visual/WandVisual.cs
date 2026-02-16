using System;
using DG.Tweening;
using UnityEngine;

public class WandVisual : MonoBehaviour, IPowerupVisual
{
    public Action OnVisualEnd { get; set; }

    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _cam;

    public void Initialize()
    {
        SignalBus.Subscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
    }

    public void PerformVisual()
    {
        _cam.SetActive(true);
        DOVirtual.DelayedCall(0.5f, () => SignalBus.Publish(new PowerUpVisualStartSignal { powerupType = PowerupType.MagicWand, OnClose = OnCancel }));
    }
    private void OnPowerVisualEnd(PowerUpVisualPanelEndSignal signal)
    {
        if (signal.powerupType != PowerupType.MagicWand) return;
        PlayEffect();
    }
    
    private void PlayEffect()
    {
        _animator.gameObject.SetActive(true);

        DOVirtual.DelayedCall(3.15f, () => OnVisualEnd?.Invoke());

        DOVirtual.DelayedCall(4.15f, () =>
        {
            _cam.SetActive(false);
            _animator.gameObject.SetActive(false);
            SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = false });
        });

    }
    private void OnCancel()
    {
        _cam.SetActive(false);
    }
    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
    }
}