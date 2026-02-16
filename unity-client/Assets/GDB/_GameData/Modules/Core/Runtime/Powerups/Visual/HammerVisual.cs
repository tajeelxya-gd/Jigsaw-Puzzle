using System;
using DG.Tweening;
using UnityEngine;

public class HammerVisual : MonoBehaviour, IPowerupVisual
{
    public Action OnVisualEnd { get; set; }

    [SerializeField] private Transform _hammer;

    public void Initialize()
    {
        SignalBus.Subscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
        SignalBus.Subscribe<OnHammerHitConfirmationSignal>(OnHammerHitConfirmationSignal);
    }

    public void PerformVisual()
    {
        SignalBus.Publish(new PowerUpVisualStartSignal { powerupType = PowerupType.Hammer, OnClose = () => SignalBus.Publish(new OnHammerEnableSignal { IsEnable = false }) });
    }

    private void OnPowerVisualEnd(PowerUpVisualPanelEndSignal signal)
    {
        if (signal.powerupType != PowerupType.Hammer) return;
        PlayHitHammer();
    }

    private float mul = 0.5f;

    private void PlayHitHammer()
    {
        if (!_hitPosition) return;
        _hammer.position = new Vector3(0, 2.7f, 0);
        _hammer.gameObject.SetActive(true);

        _hammer.DOScale(1, 1 * mul).From(0).SetEase(Ease.OutBack);

        float duration = 0.5f * mul;
        float t = 0;

        DOTween.To(() => t, x =>
        {
            t = x;
            Vector3 targetPos = _hitPosition.position + Vector3.up * 2f;
            _hammer.position = Vector3.Lerp(
                _hammer.position,
                targetPos,
                x
            );

        }, 1f, duration)
        .OnComplete(() =>
        {
            _hammer
                .DORotate(new Vector3(0, 60, -80), 1 * mul)
                .From(new Vector3(0, 60, 0))
                .SetEase(Ease.InOutCirc)
                .OnComplete(() =>
                {
                    _hammer
                        .DORotate(new Vector3(0, 60, 0), 1f * mul)
                        .From(new Vector3(0, 60, -80))
                        .SetEase(Ease.InOutCirc)
                        .OnComplete(() =>
                        {
                            _hammer.gameObject.SetActive(false);
                        });

                    _hammer.DOScale(1.5f, 0.5f * mul).From(1).SetEase(Ease.InOutCirc).OnComplete(() =>
                    {
                        _hammer.DOScale(1, 0.5f * mul).From(1.5f).SetEase(Ease.InOutCirc);
                    });
                    GlobalService.ParticleService.PlayParticle(ParticleType.HammerHit, _hammer.position - (Vector3.up * 0.2f), true);
                    OnVisualEnd?.Invoke();
                    _hitPosition = null;
                });
            _hammer.DOMove(_hammer.transform.position - new Vector3(0, 1, 0), 1f * mul);
        });
    }

    private Transform _hitPosition;
    private void OnHammerHitConfirmationSignal(OnHammerHitConfirmationSignal signal)
    {
        _hitPosition = signal.enemy.transform;
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
        SignalBus.Unsubscribe<OnHammerHitConfirmationSignal>(OnHammerHitConfirmationSignal);
    }
}
