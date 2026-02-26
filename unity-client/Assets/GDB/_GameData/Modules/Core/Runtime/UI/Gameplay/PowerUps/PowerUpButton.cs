using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour
{
    [SerializeField] private PowerupType _powerupType;
    public PowerupType PowerupType => _powerupType;

    [SerializeField] private Sprite _greySprite, _colorSprite;
    [SerializeField] private Sprite _lockSprite, _unlockSprite;
    [SerializeField] private Image _icon, _mainBG;
    [SerializeField] private TextMeshProUGUI _amountTxt, _unlockText;
    [SerializeField] private GameObject _addIcon, _amountBG, _free;
    [SerializeField] private Button _button;

    [SerializeField, Range(1, 50)] private int LevelToUnlock = 1;


    private protected int _availablePower = 0;

    public void Initialize()
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnButtonClick);
        UpdateUI();
        SignalBus.Subscribe<PowerUpAddSignal>(PowerUpAddSignal);
    }

    private void PowerUpAddSignal(PowerUpAddSignal signal)
    {
        if (signal.powerupType != _powerupType) return;
        OnPowerUpAdded(signal.Amount);
    }

    public virtual void UpdateUI()
    {
        _availablePower = GetAmount();

        if (GlobalService.GameData.Data.LevelIndex < LevelToUnlock)
            LockState();
        else
            UnlockState();
    }

    private bool _isLocked = false;
    private void LockState()
    {
        _isLocked = true;
        _unlockText.gameObject.SetActive(true);
        _unlockText.text = "Lv. " + LevelToUnlock;
        _addIcon.SetActive(false);
        _amountBG.SetActive(false);
        _icon.gameObject.SetActive(false);
        _mainBG.sprite = _lockSprite;
        _button.enabled = false;
    }

    private void UnlockState()
    {
        _isLocked = false;
        if (!GlobalService.GameData.Data.OnBoardPowerUpType.Contains(_powerupType))
        {
            ReadyForOnBoard();
            return;
        }

        _unlockText.gameObject.SetActive(false);
        _icon.gameObject.SetActive(true);
        _mainBG.sprite = _unlockSprite;

        _addIcon.SetActive(_availablePower <= 0);
        _amountBG.SetActive(_availablePower > 0);
        _free.SetActive(false);

        int previous = 0;

        if (!string.IsNullOrEmpty(_amountTxt.text))
            int.TryParse(_amountTxt.text, out previous);

        AnimateAmountChange(previous, _availablePower);

        _button.enabled = true;
        _isFree = false;
    }

    private void ReadyForOnBoard()
    {
        SignalBus.Publish(new PowerupTutorialPanelSignal { powerupType = _powerupType });
        _addIcon.SetActive(false);
        _amountBG.SetActive(false);
        _button.enabled = true;
        _unlockText.gameObject.SetActive(false);
        _icon.gameObject.SetActive(true);
        _mainBG.sprite = _unlockSprite;
    }

    private void AnimateAmountChange(int from, int to)
    {
        DOTween.Kill(_amountTxt);

        DOTween.To(
            () => from,
            value => _amountTxt.text = value.ToString(),
            to,
            0.5f
        ).SetEase(Ease.OutQuad)
         .SetTarget(_amountTxt);
    }

    private void OnButtonClick()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);

        transform.DOPunchScale(
               -new Vector3(0.2f, 0.2f, 0.2f),
               0.2f,
               10,
               0.5f
           ).OnComplete(() =>
           {
               transform.localScale = Vector3.one;
           });

        if (_isFree)
        {
            SignalBus.Publish(new OnPerformPowerUPSignal { powerupType = _powerupType });
            OnFreeClickEnd?.Invoke();
            OnFreeClickEnd = null;
            _isFree = false;
            _free.SetActive(false);
            //UpdateUI();
        }
        else if (_availablePower > 0)
        {
            SignalBus.Publish(new OnPerformPowerUPSignal { powerupType = _powerupType });
        }
        else
        {
            SignalBus.Publish(new ONPowerUpAdditionPanelSignal { powerupType = _powerupType });
            return;
        }
    }

    private bool _isFree = false;
    private Action OnFreeClickEnd;
    public void ReadyForFree(Action onClick)
    {
        OnFreeClickEnd = onClick;
        _isFree = true;
        _free.SetActive(true);
    }

    public void OnFreeClick()
    {
        _isFree = true;
        OnButtonClick();
    }

    private int GetAmount()
    {
        switch (_powerupType)
        {
            case PowerupType.Magnet: return GlobalService.GameData.Data.Magnets;
            case PowerupType.MagicWand: return GlobalService.GameData.Data.Eye;
            default: return 0;
        }
    }

    public void OnPowerUpAdded(int Amount)
    {
        IncreaseAmount(Amount);
        SignalBus.Publish(new PowerUpPlayAnimationSignal { powerupType = _powerupType, Quantity = Amount, _endTransform = this.transform, powerUpButton = this });
    }

    public void ChangeToLock()
    {
        if (_isLocked) return;
        _icon.sprite = _greySprite;
        _button.enabled = false;
    }
    public void ChangeToColor()
    {
        if (_isLocked) return;
        _icon.sprite = _colorSprite;
        _button.enabled = true;
    }
    private void IncreaseAmount(int Amount)
    {
        switch (_powerupType)
        {
            case PowerupType.Magnet: GlobalService.GameData.Data.Magnets += Amount; break;
            case PowerupType.MagicWand: GlobalService.GameData.Data.Eye += Amount; break;
            default: break;
        }
        GlobalService.GameData.Save();
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerUpAddSignal>(PowerUpAddSignal);
    }
}