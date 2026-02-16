using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpAdditionPanel : MonoBehaviour
{
    [SerializeField] private Transform _panel;
    [SerializeField] private Button _continueBtn, _crossBtn;
    [SerializeField] private Image _iconImage, _panelIconImage;
    [SerializeField] private TextMeshProUGUI _descriptionTxt, _amountTxt, _priceTxt;

    [SerializeField] private PowerUpData[] _powerUpData;

    public void Initialize()
    {
        _continueBtn.onClick.RemoveAllListeners();
        _crossBtn.onClick.RemoveAllListeners();

        _continueBtn.onClick.AddListener(PurchaseBtn);
        _crossBtn.onClick.AddListener(CrossBtn);

        _tempData = null;

        SignalBus.Subscribe<ONPowerUpAdditionPanelSignal>(OnPowerupAdditionPanelSignal);
    }

    private PowerupType _tempPowerType;
    private void OnPowerupAdditionPanelSignal(ONPowerUpAdditionPanelSignal signal)
    {
        _tempPowerType = signal.powerupType;
        ShowPanel(_tempPowerType);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<ONPowerUpAdditionPanelSignal>(OnPowerupAdditionPanelSignal);
    }

    private PowerUpData _tempData;
    [Button]
    private void ShowPanel(PowerupType powerupType)
    {
        for (int i = 0; i < _powerUpData.Length; i++)
        {
            _powerUpData[i].AnimObj.SetActive(false);
        }
        _tempData = GetPowerUpData(powerupType);
        _iconImage.sprite = _tempData.Icon;
        _panelIconImage.sprite = _tempData.HeaderIcon;
        _descriptionTxt.text = _tempData.Description;
        _amountTxt.text = "x" + _tempData.Quantity.ToString();
        _priceTxt.text = _tempData.Price.ToString();
        _tempData.AnimObj.SetActive(true);

        gameObject.SetActive(true);

        _panel.DOScale(1, 0.5f).From(0.5f).SetEase(Ease.OutBack);
        SignalBus.Publish(new InputRestrictSignal { restrict = true });
    }

    private void PurchaseBtn()
    {
        if (_tempData == null) return;
        if (_tempData.Price <= GlobalService.GameData.Data.Coins)
        {
            GlobalService.GameData.Data.Coins -= _tempData.Price;
            GlobalService.GameData.Save();
            SignalBus.Publish(new AddCoinsSignal { Amount = -_tempData.Price, IsAdd = false });
            SignalBus.Publish(new PowerUpAddSignal { powerupType = _tempData.powerupType, Amount = _tempData.Quantity });
            gameObject.SetActive(false);
        }
        else
        {
            SignalBus.Publish(new OnCoinBundleCalledSignal
            {
                OnClose = () =>
                {
                    ShowPanel(_tempPowerType);
                }
            });
        }
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        gameObject.SetActive(false);
    }

    private void CrossBtn()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        SignalBus.Publish(new InputRestrictSignal { restrict = false });
    }

    private PowerUpData GetPowerUpData(PowerupType powerupType)
    {
        for (int i = 0; i < _powerUpData.Length; i++)
        {
            if (_powerUpData[i].powerupType == powerupType) return _powerUpData[i];
        }
        return null;
    }

    [System.Serializable]
    public class PowerUpData
    {
        public PowerupType powerupType;
        public Sprite Icon;
        public Sprite HeaderIcon;
        public string Description;
        public GameObject AnimObj;
        public int Price;
        public int Quantity;
    }
}
public class ONPowerUpAdditionPanelSignal : ISignal { public PowerupType powerupType; }
public class PowerUpAddSignal : ISignal { public PowerupType powerupType; public int Amount; }