using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PowerupVisualController : MonoBehaviour
{
    [SerializeField] private PowerUpConformationPanel _powerUpConformationPanel;
    [SerializeField] private SpriteData[] spriteData;
    [SerializeField] private ConfirmationData[] confirmationData;
    [SerializeField] private GameObject _powerUpUsePanel;
    [SerializeField] private Image _bgIcon;
    [SerializeField] private Image _mainIcon;
    [SerializeField] private RectTransform _leftLine, _rightLine;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _particle;

    public void Initialize()
    {
        SignalBus.Subscribe<PowerUpVisualStartSignal>(PowerUpVisualStart);
        SignalBus.Subscribe<OnBoosterEnableSignal>(OnBoosterEnable);
        _powerUpConformationPanel.Initialize();
    }

    private System.Action _lastPowerUpCloseAction;
    private void PowerUpVisualStart(PowerUpVisualStartSignal signal)
    {
        _canvasGroup.alpha = 1;
        _powerUpConformationPanel.SetUpPanel(GetConfirmation(signal.powerupType));
        _lastPowerUpCloseAction = signal.OnClose;
        _powerUpConformationPanel.OnConfirm += OnPowerUpConfirm;
    }

    private void OnPowerUpConfirm(PowerupType powerupType)
    {
        _powerUpConformationPanel.gameObject.SetActive(false);
        Play(powerupType);
    }

    float mul = 1;
    [Button]
    private void Play(PowerupType powerupType)
    {
        _particle.SetActive(false);

        _canvasGroup.DOFade(1, 0.25f * mul).From(0).SetEase(Ease.InSine);

        _mainIcon.rectTransform.anchoredPosition = new Vector2(-630, 29.6f);
        _leftLine.anchoredPosition = new Vector2(723, 22);
        _rightLine.anchoredPosition = new Vector2(-723, 41);

        _powerUpUsePanel.SetActive(true);

        _mainIcon.sprite = GetIcon(powerupType);

        _bgIcon.DOFade(1, 0.5f * mul).From(0);
        _mainIcon.rectTransform.DOAnchorPosX(0, 2 * mul).From(new Vector2(-630, 29.6f)).SetEase(Ease.OutQuart).SetDelay(0.25f * mul);
        _rightLine.DOAnchorPosX(723 / 2, 4 * mul).From(new Vector2(-723, 41)).SetEase(Ease.OutQuart).SetDelay(0.25f * mul);
        _leftLine.DOAnchorPosX(-723 / 2, 4 * mul).From(new Vector2(723, 22)).SetEase(Ease.OutQuart).SetDelay(0.25f * mul);

        DOVirtual.DelayedCall(0.75f, () => _particle.SetActive(true));
        DOVirtual.DelayedCall(1.75f, () =>
        {
            _canvasGroup.DOFade(0, 1).SetEase(Ease.OutSine).OnComplete(() =>
            {
                SignalBus.Publish(new PowerUpVisualPanelEndSignal { powerupType = powerupType });
                _powerUpUsePanel.SetActive(false);
            });
        });
    }

    private Sprite GetIcon(PowerupType powerupType)
    {
        for (int i = 0; i < spriteData.Length; i++)
        {
            if (spriteData[i].powerupType == powerupType) return spriteData[i].sprite;
        }
        return spriteData[0].sprite;
    }

    private ConfirmationData GetConfirmation(PowerupType powerupType)
    {
        for (int i = 0; i < spriteData.Length; i++)
        {
            if (spriteData[i].powerupType == powerupType) return confirmationData[i];
        }
        return confirmationData[0];
    }

    private void OnBoosterEnable(OnBoosterEnableSignal signal)
    {
        if (signal.IsEnable) return;
        _lastPowerUpCloseAction?.Invoke();
        _powerUpConformationPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _powerUpConformationPanel.OnConfirm -= OnPowerUpConfirm;
        SignalBus.Unsubscribe<PowerUpVisualStartSignal>(PowerUpVisualStart);
        SignalBus.Unsubscribe<OnBoosterEnableSignal>(OnBoosterEnable);
    }

    [System.Serializable]
    public struct SpriteData
    {
        public PowerupType powerupType;
        public Sprite sprite;
    }
    [System.Serializable]
    public struct ConfirmationData
    {
        public PowerupType powerupType;
        public string Title, Description, ButtonText;
        public float BGYAnchorPos;
        public float ButtonYAnchorPos;
        public bool ShowButton;
    }
}