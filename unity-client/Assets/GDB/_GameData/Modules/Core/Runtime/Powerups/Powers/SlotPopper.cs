using System;
using UnityEngine;

public class SlotPopper : IPowerup, IDisposable
{
    public PowerupType powerupType { get { return PowerupType.SlotPopper; } }

    private IPowerupVisual _powerupVisual;
    private SpaceController _spaceController;
    public SlotPopper(IPowerupVisual powerupVisual, SpaceController spaceController)
    {
        _powerupVisual = powerupVisual;
        _spaceController = spaceController;
        _powerupVisual.OnVisualEnd += OnPerformEnd;
    }
    public void PerformPowerUp()
    {
        _powerupVisual.PerformVisual();
    }
    private void OnPerformEnd()
    {
        _spaceController.Pop_Shooter();
        AudioController.PlayOnDemandSFX(AudioType.SlotPopperEffect);
        HapticController.Vibrate(HapticType.Hammer);
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseSlotPopper, Amount = 1 });
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.UseBooster, Amount = 1 });
        if (TutorialManager.IsTutorialActivated)
            SignalBus.Publish(new OnTutorialActivated { IsActivated = false });
        GameAnalytics.SlotPopperUsed++;
    }

    public void Dispose()
    {
        _powerupVisual.OnVisualEnd -= OnPerformEnd;
    }
}
