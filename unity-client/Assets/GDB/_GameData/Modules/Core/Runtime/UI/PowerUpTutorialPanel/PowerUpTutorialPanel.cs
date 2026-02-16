using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpTutorialPanel : MonoBehaviour
{
    [SerializeField] private Transform _panel;
    [SerializeField] private Button _continueBtn, _crossBtn;
    [SerializeField] private Image _iconImage, _panelIconImage;
    [SerializeField] private TextMeshProUGUI _descriptionTxt;

    [SerializeField] private PowerUpData[] _powerUpData;

    public void Initialize()
    {
        _continueBtn.onClick.RemoveAllListeners();
        _crossBtn.onClick.RemoveAllListeners();

        _continueBtn.onClick.AddListener(Continue);
        _crossBtn.onClick.AddListener(Continue);

        SignalBus.Subscribe<PowerupTutorialPanelSignal>(OnPowerupTutorialPanelSignal);
    }

    private PowerupType _tempPowerType;
    private void OnPowerupTutorialPanelSignal(PowerupTutorialPanelSignal signal)
    {
        _tempPowerType = signal.powerupType;
        ShowTutorialPanel(_tempPowerType);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerupTutorialPanelSignal>(OnPowerupTutorialPanelSignal);
    }

    [Button]
    private void ShowTutorialPanel(PowerupType powerupType)
    {
        for (int i = 0; i < _powerUpData.Length; i++)
        {
            _powerUpData[i].AnimObj.SetActive(false);
        }
        PowerUpData data = GetPowerUpData(powerupType);
        _iconImage.sprite = data.Icon;
        _panelIconImage.sprite = data.HeaderIcon;
        _descriptionTxt.text = data.Description;
        data.AnimObj.SetActive(true);

        gameObject.SetActive(true);

        _panel.DOScale(1, 0.5f).From(0.5f).SetEase(Ease.OutBack);
        SignalBus.Publish(new InputRestrictSignal { restrict = true });
    }

    private void Continue()
    {
        SignalBus.Publish(new OnBoosterTutorialShownSignal { powerupType = _tempPowerType });
        // SignalBus.Publish(new InputRestrictSignal { restrict = false });
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        gameObject.SetActive(false);
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
    }
}