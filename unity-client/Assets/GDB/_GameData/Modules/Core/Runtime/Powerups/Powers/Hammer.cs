using System;
using UnityEngine;

public class Hammer : IPowerup, IDisposable
{
    public PowerupType powerupType { get { return PowerupType.Hammer; } }

    private IPowerupVisual _powerupVisual;
    public Hammer(IPowerupVisual powerupVisual)
    {
        _powerupVisual = powerupVisual;
        _powerupVisual.OnVisualEnd += OnPerformEnd;
        SignalBus.Subscribe<OnHammerHitConfirmationSignal>(OnHammerHitConfirmationSignal);
    }

    public void PerformPowerUp()
    {
        SignalBus.Publish(new OnHammerEnableSignal { IsEnable = true });
        _powerupVisual.PerformVisual();
    }
    private void OnPerformEnd()
    {
        if (TutorialManager.IsTutorialActivated)
            SignalBus.Publish(new OnTutorialActivated { IsActivated = false });

        if (_onHammerHitConfirmationSignal != null)
            SignalBus.Publish(new DestroyArmyThroughHammer
            {
                colorType = _onHammerHitConfirmationSignal.colorType,
                IsBoss = _onHammerHitConfirmationSignal.IsBoss,
                enemy = _onHammerHitConfirmationSignal.enemy
            });
        _onHammerHitConfirmationSignal = null;
        SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = false });
        SignalBus.Publish(new OnHammerEnableSignal { IsEnable = false });
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseHammer, Amount = 1 });
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseBooster, Amount = 1 });
        AudioController.PlayOnDemandSFX(AudioType.HammerEffect);
        HapticController.Vibrate(HapticType.Hammer);
        GameAnalytics.HammerUsed++;
    }

    private OnHammerHitConfirmationSignal _onHammerHitConfirmationSignal;
    private void OnHammerHitConfirmationSignal(OnHammerHitConfirmationSignal signal)
    {
        _onHammerHitConfirmationSignal = signal;
    }

    public void Dispose()
    {
        _powerupVisual.OnVisualEnd -= OnPerformEnd;
        SignalBus.Unsubscribe<OnHammerHitConfirmationSignal>(OnHammerHitConfirmationSignal);
    }
}
