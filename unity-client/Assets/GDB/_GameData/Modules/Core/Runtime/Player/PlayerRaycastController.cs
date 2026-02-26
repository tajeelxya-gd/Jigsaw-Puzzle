using System;
using UnityEngine;

public class PlayerRaycastController : MonoBehaviour, ITickable
{
    [SerializeField] private LayerMask _interectionLayer;

    private Camera _mainCam;
    private SpaceController _spaceController;
    private CannonController _cannonController;

    private void Start()
    {
        SignalBus.Subscribe<OnTutorialActivated>(OnTutorialActivated);
    }

    public void Initilize(SpaceController spaceController, CannonController cannonController)
    {
        Input.multiTouchEnabled = false;
        _mainCam = Camera.main;
        _spaceController = spaceController;
        _cannonController = cannonController;


        _canClick = true;

        SignalBus.Subscribe<OnBoosterEnableSignal>(OnBoosterSignal);
        SignalBus.Subscribe<PowerUpVisualStartSignal>(OnPowerUpVisual);
        SignalBus.Subscribe<InputRestrictSignal>(InputRestrictSignal);
    }

    private bool _touchRestricted = false;
    public bool CanClick
    {
        get
        {
            return _canClick;
        }
        set
        {
            _canClick = value;
        }
    }
    private bool _canClick = true;
    public void Tick()
    {
        if (_touchRestricted) return; //only for tutorial
        if (!_canClick) return;
        if (Input.GetMouseButtonDown(0))
        {
            OnThrowRaycast();
        }
    }

    void OnThrowRaycast()
    {
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50, _interectionLayer))
        {
            IInterectable interactable = hit.collider.GetComponent<IInterectable>();
            if (interactable != null)
            {
                if (_isHammerPowerUpEnable)
                {
                    if (interactable is Enemy enemy)
                    {
                        SignalBus.Publish(new OnHammerHitConfirmationSignal { colorType = enemy.ColorType, IsBoss = enemy.Health > 2, enemy = enemy });
                        OnBoosterSignal(new OnBoosterEnableSignal { IsEnable = false });
                        _canClick = false;
                    }
                    return;
                }

                if (interactable.CanInterect)
                {
                    IShootable shootable = interactable as IShootable;
                    if (shootable != null)
                    {
                        ILink<Cannon> link = interactable as ILink<Cannon>;

                        if (_isMagnetEnable)
                        {
                            if (link.IsLinked) return;
                            if (!GlobalService.GameData.Data.OnBoardPowerUpType.Contains(PowerupType.Magnet))
                                shootable = _cannonController.GetInterectableForMagnetOnBoard();
                            SignalBus.Publish(new OnMagnetConfirmationSignal { shootable = shootable, spaceController = _spaceController });
                            SignalBus.Publish(new OnMagnetEnableSignal { IsEnable = false });
                            OnBoosterSignal(new OnBoosterEnableSignal { IsEnable = false });
                            return;
                        }

                        if (link.IsLinked && link.LinkedObject)
                        {
                            ILink<Cannon> link2 = link.LinkedObject as ILink<Cannon>;
                            if (link.IsLinked && link.AllFree && link2.AllFree && _spaceController.FindAvailableAdjacentPair(out Space space1, out Space space2))
                            {
                                space1.SetOccupance(shootable);
                                space2.SetOccupance(link.LinkedObject);
                                SignalBus.Publish(new OnShooterRemoveFromGridSignal { shootable = new System.Collections.Generic.List<IShootable> { shootable, link.LinkedObject } });
                                AudioController.PlayOnDemandSFX(AudioType.CannonTouch);
                                HapticController.Vibrate(HapticType.CannonTouch);
                                return;
                            }
                        }
                        else if (_spaceController.isSpaceAvailable(out Space space))
                        {
                            space.SetOccupance(shootable);
                            SignalBus.Publish(new OnShooterRemoveFromGridSignal { shootable = new System.Collections.Generic.List<IShootable> { shootable } });
                            AudioController.PlayOnDemandSFX(AudioType.CannonTouch);
                            HapticController.Vibrate(HapticType.CannonTouch);
                            return;
                        }
                    }
                }
                OnItemReject(interactable);
            }
        }
    }

    public void OnMouseDown_TutorialOnly()
    {
        OnThrowRaycast();
    }

    void OnTutorialActivated(OnTutorialActivated signal)
    {
        _touchRestricted = signal.IsActivated && signal.OverrideInput;
    }

    private void OnItemReject<T>(T obj)
    {
        IRejection rejection = obj as IRejection;
        if (rejection != null)
            rejection.Reject();
    }

    private void InputRestrictSignal(InputRestrictSignal signal)
    {
        CanClick = !signal.restrict;
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnBoosterEnableSignal>(OnBoosterSignal);
        SignalBus.Unsubscribe<PowerUpVisualStartSignal>(OnPowerUpVisual);
        SignalBus.Unsubscribe<OnTutorialActivated>(OnTutorialActivated);
        SignalBus.Unsubscribe<InputRestrictSignal>(InputRestrictSignal);
    }

    private bool _isHammerPowerUpEnable = false;
    private bool _isMagnetEnable = false;

    private void OnBoosterSignal(OnBoosterEnableSignal signal)
    {
        if (!signal.IsEnable)
        {
            _canClick = true;
            _isHammerPowerUpEnable = false;
            _isMagnetEnable = false;
        }
    }
    private void OnPowerUpVisual(PowerUpVisualStartSignal signal)
    {
        if (signal.powerupType == PowerupType.MagicWand || signal.powerupType == default)
        {
            _canClick = false;
            return;
        }
        else if (signal.powerupType == default)
            _isHammerPowerUpEnable = true;
        else if (signal.powerupType == PowerupType.Magnet)
            _isMagnetEnable = true;
    }
}
public class InputRestrictSignal : ISignal
{
    public bool restrict;
}