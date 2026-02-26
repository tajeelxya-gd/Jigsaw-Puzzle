using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpConformationPanel : MonoBehaviour
{
    [SerializeField] private RectTransform _InFoBar;
    [SerializeField] private RectTransform _actionBtn;
    [SerializeField] private GameObject _maskParent;
    [SerializeField] private GameObject _magnetMask, _popMask, _hammerMask, _shuffleMask;
    [SerializeField] private Text _infoTitle, _infoDescription;
    [SerializeField] private TextMeshProUGUI _actionBtnText;
    [SerializeField] private Button _crossBtn;

    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private RectTransform _hand;

    [SerializeField] private HandData[] _handData;


    public Action<PowerupType> OnConfirm;

    private PowerupType _currentPowerUpType;

    public void Initialize()
    {
        SignalBus.Subscribe<OnHammerHitConfirmationSignal>(OnHammerHitConfirmation);
        SignalBus.Subscribe<OnMagnetConfirmationSignal>(OnMagnetConfirmation);
        OnConfirm = null;
    }

    public void SetUpPanel(PowerupVisualController.ConfirmationData confirmationData)
    {
        _currentPowerUpType = confirmationData.powerupType;

        _infoTitle.text = confirmationData.Title;
        _infoDescription.text = confirmationData.Description;
        _actionBtnText.text = confirmationData.ButtonText;
        _InFoBar.anchoredPosition = new Vector2(0, confirmationData.BGYAnchorPos);
        if (confirmationData.ShowButton)
            _actionBtn.anchoredPosition = new Vector2(0, confirmationData.ButtonYAnchorPos);
        _actionBtn.gameObject.SetActive(confirmationData.ShowButton);
        if (confirmationData.ShowButton)
            _actionBtn.gameObject.transform.DOScale(1, 1).From(0).SetEase(Ease.OutBack);

        _magnetMask.SetActive(confirmationData.powerupType == PowerupType.Magnet);
        _shuffleMask.SetActive(confirmationData.powerupType == PowerupType.MagicWand);

        _maskParent.SetActive(true);

        gameObject.SetActive(true);

        _InFoBar.gameObject.transform.DOScale(1, 1).From(0).SetEase(Ease.OutBack);
        _canvasGroup.DOFade(1, 1).From(0);

        _crossBtn.gameObject.SetActive(GlobalService.GameData.Data.OnBoardPowerUpType.Contains(_currentPowerUpType));

        _hand.gameObject.SetActive(!GlobalService.GameData.Data.OnBoardPowerUpType.Contains(_currentPowerUpType));
        if (!GlobalService.GameData.Data.OnBoardPowerUpType.Contains(_currentPowerUpType))
        {
            HandData handData = GetHandData(confirmationData.powerupType);
            _hand.anchoredPosition = handData.AnchorPos;
            _hand.eulerAngles = new Vector3(handData.AnchorRot.x, 0, handData.AnchorRot.y);
        }
    }

    private HandData GetHandData(PowerupType powerupType)
    {
        for (int i = 0; i < _handData.Length; i++)
        {
            if (_handData[i].powerupType == powerupType) return _handData[i];
        }
        return null;
    }

    public void ConfirmAction()
    {
        OnPowerUpUse();
        OnConfirm?.Invoke(_currentPowerUpType);
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
    }

    private void ReducePowerUP(int n, PowerupType powerupType)
    {
        switch (powerupType)
        {
            case PowerupType.MagicWand: GlobalService.GameData.Data.Wand -= n; break;
            case PowerupType.Magnet: GlobalService.GameData.Data.Magnets -= n; break;
            default: break;
        }
        GlobalService.GameData.Save();
    }

    public void EndPowerUp()
    {
        SignalBus.Publish(new OnBoosterEnableSignal { IsEnable = false });
    }

    private void OnHammerHitConfirmation(ISignal signal)
    {
        OnPowerUpUse();
        OnConfirm?.Invoke(_currentPowerUpType);
    }

    private void OnMagnetConfirmation(ISignal signal)
    {
        OnPowerUpUse();
        OnConfirm?.Invoke(_currentPowerUpType);
    }

    private void OnPowerUpUse()
    {
        if (!GlobalService.GameData.Data.OnBoardPowerUpType.Contains(_currentPowerUpType))
        {
            GlobalService.GameData.Data.OnBoardPowerUpType.Add(_currentPowerUpType);
            GlobalService.GameData.Save();
        }
        else
            ReducePowerUP(1, _currentPowerUpType);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnHammerHitConfirmationSignal>(OnHammerHitConfirmation);
        SignalBus.Unsubscribe<OnMagnetConfirmationSignal>(OnMagnetConfirmation);
    }

    [System.Serializable]
    public class HandData
    {
        public PowerupType powerupType;
        public Vector2 AnchorPos;
        public Vector2 AnchorRot;
    }
}