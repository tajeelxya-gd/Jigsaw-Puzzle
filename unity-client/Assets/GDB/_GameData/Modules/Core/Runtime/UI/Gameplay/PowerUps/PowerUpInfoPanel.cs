using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpInfoPanel : MonoBehaviour
{
    [SerializeField] private Transform _panel;
    [SerializeField] private RectTransform _arrowRect;
    [SerializeField] private Button _crossBtn;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private RectTransform _handObj;
    [SerializeField] private Data[] _boosterData;

    private PowerUpButtonController _powerUpButtonController;

    public void Initialize(PowerUpButtonController powerUpButtonController)
    {
        _powerUpButtonController = powerUpButtonController;
        _handObj.gameObject.SetActive(false);
        _crossBtn.onClick.AddListener(OnCross);
        SignalBus.Subscribe<OnBoosterTutorialShownSignal>(OnBoosterTutorialShownSignal);
    }

    private PowerUpButton _tempPowerUpButton;

    private void OnBoosterTutorialShownSignal(OnBoosterTutorialShownSignal signal)
    {
        gameObject.SetActive(true);
        _panel.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBack);
        Data data = GetData(signal.powerupType);
        _iconImage.sprite = data.Icon;
        _arrowRect.anchoredPosition = new Vector2(data.ArrowPos.x, data.ArrowPos.y);
        _arrowRect.eulerAngles = new Vector2(0, data.YRot);
        _handObj.anchoredPosition = new Vector2(data.HandPos.x, data.HandPos.y);
        _handObj.eulerAngles = new Vector3(data.HandRot.x, data.HandRot.y, data.HandRot.z);
        _descriptionText.text = data.Text;
        _tempPowerUpButton = _powerUpButtonController.GetButton(signal.powerupType);
        _tempPowerUpButton.transform.SetParent(transform);
        _tempPowerUpButton.ReadyForFree(OnPowerUpStart);
        _handObj.SetAsLastSibling();
        _handObj.gameObject.SetActive(true);
        _panelCalled = false;
        SignalBus.Subscribe<OnBoosterEnableSignal>(OnBoosterSignal);
        SignalBus.Publish(new InputRestrictSignal { restrict = true });
    }

    private void OnCross()
    {
        _tempPowerUpButton.OnFreeClick();
    }

    private void OnPowerUpStart()
    {
        _tempPowerUpButton.transform.SetParent(_powerUpButtonController.transform);
        SignalBus.Publish(new InputRestrictSignal { restrict = false });
        gameObject.SetActive(false);
    }

    private bool _panelCalled = false;
    private void OnBoosterSignal(OnBoosterEnableSignal signal)
    {
        if (_panelCalled) return;
        if (signal.IsEnable) return;
        _panelCalled = true;
        DOVirtual.DelayedCall(1, () =>
           {
               if (_tempPowerUpButton)
               {
                   _tempPowerUpButton.OnPowerUpAdded(3);
               }
           });
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnBoosterEnableSignal>(OnBoosterSignal);
        SignalBus.Unsubscribe<OnBoosterTutorialShownSignal>(OnBoosterTutorialShownSignal);
    }

    private Data GetData(PowerupType powerupType)
    {
        for (int i = 0; i < _boosterData.Length; i++)
        {
            if (_boosterData[i].powerupType == powerupType) return _boosterData[i];
        }
        return null;
    }

    [System.Serializable]
    public class Data
    {
        public PowerupType powerupType;
        public Sprite Icon;
        public string Text;
        public Vector2 ArrowPos;
        public float YRot;
        public Vector2 HandPos;
        public Vector3 HandRot;
    }
}