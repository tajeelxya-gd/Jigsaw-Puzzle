using System;
using Lofelt.NiceVibrations;
using UnityEngine;

public class Magnet : IPowerup, IDisposable
{
    public PowerupType powerupType { get { return PowerupType.Magnet; } }

    private IPowerupVisual _powerupVisual;
    public Magnet(IPowerupVisual powerupVisual)
    {
        _powerupVisual = powerupVisual;
        _powerupVisual.OnVisualEnd += OnPerformEnd;
        SignalBus.Subscribe<OnMagnetConfirmationSignal>(OnMagnetConfirmationSignal);
    }

    public void PerformPowerUp()
    {
        SignalBus.Publish(new OnMagnetEnableSignal { IsEnable = true });
        _powerupVisual.PerformVisual();
    }

    private void OnPerformEnd()
    {
        if (!_spaceController) return;
        if (_spaceController.isSpaceAvailable(out Space space))
        {
            space.SetOccupance(_shootable);
            SignalBus.Publish(new OnShooterRemoveFromGridSignal { shootable = new System.Collections.Generic.List<IShootable> { _shootable } });
            AudioController.PlaySFX(AudioType.MagnetEffect);
            HapticController.Vibrate(HapticType.Hammer);
            Debug.LogError("Publishing signal here");
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseSelect, Amount = 1 });
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseBooster, Amount = 1 });
        }

        _shootable = null;
        _spaceController = null;
       
        // SignalBus.Publish(new OnMagnetEnableSignal { IsEnable = false });
        if (TutorialManager.IsTutorialActivated)
            SignalBus.Publish(new OnTutorialActivated { IsActivated = false });
        GameAnalytics.MagnetUsed++;
    }

    private IShootable _shootable;
    private SpaceController _spaceController;
    private void OnMagnetConfirmationSignal(OnMagnetConfirmationSignal signal)
    {
        _shootable = signal.shootable;
        _spaceController = signal.spaceController;
    }

    public void Dispose()
    {
        _powerupVisual.OnVisualEnd = null;
        SignalBus.Unsubscribe<OnMagnetConfirmationSignal>(OnMagnetConfirmationSignal);
    }
}
