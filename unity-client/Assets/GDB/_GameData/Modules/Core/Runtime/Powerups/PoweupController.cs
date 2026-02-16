using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PoweupController : MonoBehaviour
{
    [SerializeField] private HammerVisual _hammerVisual;
    [SerializeField] private MagnetVisual _magnetVisual;
    [SerializeField] private SlotPoperVisual _slotPopperVisual;
    [SerializeField] private WandVisual _wandVisual;

    private HashSet<IPowerup> _powerups = new HashSet<IPowerup>();

    private CannonController _cannonController;
    private EnemyController _enemyController;
    private SpaceController _spaceController;
    public void Initialize(CannonController cannonController, EnemyController enemyController, SpaceController spaceController)
    {
        _cannonController = cannonController;
        _enemyController = enemyController;
        _spaceController = spaceController;

        _wandVisual.Initialize();
        _hammerVisual.Initialize();
        _magnetVisual.Initialize();
        _slotPopperVisual.Initialize(spaceController);

        DisposePowerups();

        _powerups = new HashSet<IPowerup> { new Hammer(_hammerVisual), new Magnet(_magnetVisual),
        new SlotPopper(_slotPopperVisual,_spaceController), new Wand(_wandVisual,_cannonController)};

        SignalBus.Subscribe<OnPerformPowerUPSignal>(OnPerformPowerUP);
    }

    private void OnPerformPowerUP(OnPerformPowerUPSignal signal)
    {
        PerformPowerUp(signal.powerupType);
    }
    [Button]
    private void PerformPowerUp(PowerupType powerupType)
    {
        foreach (var powerup in _powerups)
        {
            if (powerup.powerupType == powerupType)
            {
                SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = true });
                powerup.PerformPowerUp();
                break;
            }
        }
    }
    private void DisposePowerups()
    {
        foreach (var powerup in _powerups)
        {
            if (powerup is IDisposable disposable)
                disposable.Dispose();
        }

        _powerups.Clear();
    }
}
public class OnPerformPowerUPSignal : ISignal
{
    public PowerupType powerupType;
}