using DG.Tweening;
using UnityEngine;

public class PowerUpButtonController : MonoBehaviour
{
    [SerializeField] private RectTransform _mainRect;
    [SerializeField] private PowerUpButton[] _powerUpButtons;

    private SpaceController _spaceController;
    private CannonController _cannonController;

    public void Initialize()
    {
        SignalBus.Subscribe<OnShooterEmptySignal>(EnableMagnet);
        SignalBus.Subscribe<OnShooterEmptySignal>(DisableSlotPopper);
        SignalBus.Subscribe<OnBoosterEnableSignal>(OnBoosterEnable);
        SignalBus.Subscribe<OnShooterRemoveFromGridSignal>(OnShooterRemoveFromGridSignal);
        for (int i = 0; i < _powerUpButtons.Length; i++)
        {
            _powerUpButtons[i].Initialize();

            if (_powerUpButtons[i].PowerupType == PowerupType.Magnet)
                _magnetPowerUpBtn = _powerUpButtons[i];
            else if (_powerUpButtons[i].PowerupType == PowerupType.MagicWand)
                _shufflePowerUpBtn = _powerUpButtons[i];
        }
        _magnetPowerUpBtn.ChangeToColor();
        _shufflePowerUpBtn.ChangeToColor();
        _hammerPowerUpBtn.ChangeToColor();
        if (GlobalService.GameData.Data.LevelIndex == 25)
        {
            _slotPowerUpBtn.ChangeToColor();
        }
        else
        {
            _slotPowerUpBtn.ChangeToLock();
        }
    }

    private void OnBoosterEnable(OnBoosterEnableSignal onBoosterEnable)
    {
        MoveYPos(onBoosterEnable.IsEnable ? -350 : 56);
        if (!onBoosterEnable.IsEnable) UpdateUI();
    }

    private void MoveYPos(float pos)
    {
        _mainRect.DOAnchorPosY(pos, 1).SetEase(Ease.OutCirc);
    }

    private void UpdateUI()
    {
        for (int i = 0; i < _powerUpButtons.Length; i++)
        {
            _powerUpButtons[i].UpdateUI();
        }
    }

    public PowerUpButton GetButton(PowerupType powerupType)
    {
        for (int i = 0; i < _powerUpButtons.Length; i++)
        {
            if (_powerUpButtons[i].PowerupType == powerupType) return _powerUpButtons[i];
        }

        return null;
    }

    private PowerUpButton _magnetPowerUpBtn;
    private PowerUpButton _shufflePowerUpBtn;
    private PowerUpButton _hammerPowerUpBtn;
    private PowerUpButton _slotPowerUpBtn;

    private void OnShooterRemoveFromGridSignal(OnShooterRemoveFromGridSignal signal)
    {
        if (!_cannonController.HasCannon())
        {
            _magnetPowerUpBtn.ChangeToLock();
            _shufflePowerUpBtn.ChangeToLock();
            _hammerPowerUpBtn.ChangeToLock();
            _slotPowerUpBtn.ChangeToLock();
        }
        else
        {
            _magnetPowerUpBtn.ChangeToColor();
            _shufflePowerUpBtn.ChangeToColor();
            _hammerPowerUpBtn.ChangeToColor();
            _slotPowerUpBtn.ChangeToColor();
        }

        //SLOT POPPER
        if (_spaceController.AnySpaceOccupied() && _cannonController.HasCannon())
        {
            _slotPowerUpBtn.ChangeToColor();
        }
        else if (!_spaceController.AnySpaceOccupied() && _cannonController.HasCannon())
        {
            _slotPowerUpBtn.ChangeToLock();
        }
        else
        {
            _slotPowerUpBtn.ChangeToLock();
        }

        //MAGNET 
        if (!_cannonController.HasCannon() || _spaceController.AllSpacesFull(5))
        {
            _magnetPowerUpBtn.ChangeToLock();
        }
        else
        {
            _magnetPowerUpBtn.ChangeToColor();
        }
    }

    private void EnableMagnet(OnShooterEmptySignal signal)
    {
        if (!_cannonController.HasCannon()) return;
        _magnetPowerUpBtn.ChangeToColor();
    }

    private void DisableSlotPopper(OnShooterEmptySignal signal)
    {
        if (!_spaceController.AnySpaceOccupied() && _cannonController.HasCannon())
        {
            _slotPowerUpBtn.ChangeToLock();
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnBoosterEnableSignal>(OnBoosterEnable);
        SignalBus.Unsubscribe<OnShooterRemoveFromGridSignal>(OnShooterRemoveFromGridSignal);
        SignalBus.Unsubscribe<OnShooterEmptySignal>(EnableMagnet);
        SignalBus.Unsubscribe<OnShooterEmptySignal>(DisableSlotPopper);
    }
}