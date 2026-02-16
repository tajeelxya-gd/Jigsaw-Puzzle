using System;
using DG.Tweening;
using UnityEngine;

public class MagnetVisual : MonoBehaviour, IPowerupVisual
{
    public Action OnVisualEnd { get; set; }

    [SerializeField] private GameObject _cam;
    [SerializeField] private Transform _magnet;
    [SerializeField] private ParticleSystem _electricLongEffect, _electricHitEffect;

    private Vector3 _originalPos;

    public void Initialize()
    {
        SignalBus.Subscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
        SignalBus.Subscribe<OnMagnetConfirmationSignal>(OnMagnetConfirmationSignal);
        _originalPos = _magnet.transform.position;
    }

    public void PerformVisual()
    {
        _cam.SetActive(true);
        DOVirtual.DelayedCall(0.5f, () => SignalBus.Publish(new PowerUpVisualStartSignal { powerupType = PowerupType.Magnet, OnClose = OnCancel }));
    }

    private void OnPowerVisualEnd(PowerUpVisualPanelEndSignal signal)
    {
        if (signal.powerupType != PowerupType.Magnet) return;
        PlayEffect();
    }
    float mul = 1;
    private void PlayEffect()
    {
        _magnet.gameObject.SetActive(true);
        _magnet.transform.position = _originalPos;
        _magnet.DOScale(1, 0.25f * mul)
            .SetEase(Ease.OutBack)
            .From(0)
            .OnComplete(() =>
            {
                if (_shootable == null) return;

                _electricLongEffect.gameObject.SetActive(true);
                _electricLongEffect.Play();

                _electricHitEffect.gameObject.SetActive(true);
                _electricHitEffect.Play();

                _magnet.DOMoveZ(-8, 1).From(-3).SetEase(Ease.OutSine);
                OnCancel();
                OnVisualEnd?.Invoke();
                SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = false });
                StartTracking();
            });
    }
    private Tween _trackTween;
    private Tween _stopTween;
    private void StartTracking()
    {
        _trackTween?.Kill();
        _stopTween?.Kill();

        LoopTrack();
        _stopTween = DOVirtual.DelayedCall(1.25f, () =>
        {
            _trackTween?.Kill();
            _trackTween = null;
            _electricLongEffect.gameObject.SetActive(false);
            _electricHitEffect.gameObject.SetActive(false);
            _magnet.DOScale(0, 0.25f * mul)
            .SetEase(Ease.OutBack)
            .From(1);
        });
    }

    private void LoopTrack()
    {
        _trackTween = DOVirtual.DelayedCall(0f, () =>
        {
            if (_shootable == null || _magnet == null)
                return;

            Vector3 dir = _shootable.position - _magnet.position;
            _magnet.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180, 0);

            _electricHitEffect.transform.position = _shootable.position;
            LoopTrack();

        }).SetUpdate(true);
    }

    private void OnCancel()
    {
        _cam.SetActive(false);
        SignalBus.Publish(new OnMagnetEnableSignal { IsEnable = false });
    }

    private Transform _shootable;
    private void OnMagnetConfirmationSignal(OnMagnetConfirmationSignal signal)
    {
        if (signal.shootable is MonoBehaviour monoBehaviour)
        {
            _shootable = monoBehaviour.gameObject.transform;
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
        SignalBus.Unsubscribe<OnMagnetConfirmationSignal>(OnMagnetConfirmationSignal);
    }
}