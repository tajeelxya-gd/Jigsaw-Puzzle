using System;
using UnityEngine;

public class Wand : IPowerup, IDisposable
{
    public PowerupType powerupType { get { return PowerupType.MagicWand; } }
    private IPowerupVisual _powerupVisual;
    private CannonController _cannonController;
    public Wand(IPowerupVisual powerupVisual, CannonController cannonController)
    {
        _powerupVisual = powerupVisual;
        _cannonController = cannonController;
        _powerupVisual.OnVisualEnd += OnPerformEnd;
    }
    public void PerformPowerUp()
    {
        _powerupVisual.PerformVisual();
    }

    private void OnPerformEnd()
    {
        _cannonController.Shuffle();
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = default, Amount = 1 });
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseBooster, Amount = 1 });
        if (TutorialManager.IsTutorialActivated)
            SignalBus.Publish(new OnTutorialActivated { IsActivated = false });
        AudioController.PlayOnDemandSFX(AudioType.ShuffleEffect);
        GameAnalytics.ShufflerUsed++;
    }

    public void Dispose()
    {
        _powerupVisual.OnVisualEnd -= OnPerformEnd;
    }
}
