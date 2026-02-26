using System;
using DG.Tweening;
using UnityEngine;
public class SlotPoperVisual : MonoBehaviour, IPowerupVisual
{
    public Action OnVisualEnd { get; set; }

    [SerializeField] private Transform _mainJesterBox, _jesterHandle, _spring, _jesterTop;
    [SerializeField] private ParticleSystem _enchantmentParticle;

    private SpaceController _spaceController;
    public void Initialize(SpaceController spaceController)
    {
        _spaceController = spaceController;
        SignalBus.Subscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
    }

    public void PerformVisual()
    {
        SignalBus.Publish(new PowerUpVisualStartSignal { powerupType = default, OnClose = null });
    }

    private void OnPowerVisualEnd(PowerUpVisualPanelEndSignal signal)
    {
        if (signal.powerupType != default) return;
        PlayEffect();
    }

    float mul = 0.75f;
    private void PlayEffect()
    {
        Space space = _spaceController.GetRandomSpaceForPop();
        if (space == null)
        {
            SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = false }); return;
        }

        _mainJesterBox.transform.position = new Vector3(0, 0.55f, -1.12f);
        _mainJesterBox.gameObject.SetActive(true);

        _enchantmentParticle.gameObject.SetActive(true);
        _enchantmentParticle.Play();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_jesterTop.DOLocalRotate(new Vector3(-33, 0, 0), 1 * mul).From(new Vector3(140, 0, 0)).SetEase(Ease.OutSine)).Join(
            _jesterHandle.DOLocalRotate(new Vector3(720, 180, 0), 1 * mul, RotateMode.FastBeyond360).From(new Vector3(0, 180, 0)).SetEase(Ease.OutSine)
        ).Append(_mainJesterBox.DOJump(space.transform.position, 5, 1, 1f * mul).SetEase(Ease.OutSine)).Join(_mainJesterBox.DOScale(10, 1f * mul).From(15).SetEase(Ease.OutSine))
        .Join(_mainJesterBox.DOLocalRotate(new Vector3(0, 180, 0), 1 * mul, RotateMode.FastBeyond360).From(new Vector3(33, 180, 0)).SetEase(Ease.OutSine)).
        Append(_jesterTop.DOLocalRotate(new Vector3(140, 0, 0), 0.5f * mul).SetEase(Ease.OutSine)).
        Join(_jesterHandle.DOLocalRotate(new Vector3(0, 180, 0), 0.5f * mul, RotateMode.FastBeyond360).SetEase(Ease.OutSine).OnComplete(() =>
        {
            GlobalService.ParticleService.PlayParticle(ParticleType.TreasureOpen, _mainJesterBox.position + (Vector3.up * 1f), true);
            OnVisualEnd?.Invoke();
            SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = false });
        })).
        Append(_spring.DOScaleY(1.2f, 0.3f * mul).From(0).SetEase(Ease.OutBack)).Append(_spring.DOScaleY(0.65f, 0.25f * mul).SetEase(Ease.InBack)).OnComplete(() =>
        {
            _mainJesterBox.DOScale(0, 0.5f * mul).SetEase(Ease.OutCirc).OnComplete(() =>
            {
                _mainJesterBox.gameObject.SetActive(false);
            });
        });
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerUpVisualPanelEndSignal>(OnPowerVisualEnd);
    }
}