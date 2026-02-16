using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpAdditionAnim : AnimationPoolManager
{
    [SerializeField] private IconData[] _iconData;
    private protected override void Start()
    {
        base.Start();
        SignalBus.Subscribe<PowerUpPlayAnimationSignal>(PowerUpPlayAnimationSignal);
    }

    private void PowerUpPlayAnimationSignal(PowerUpPlayAnimationSignal signal)
    {
        ChangeObjectsSprites(GetPowerUpSprite(signal.powerupType));
        SetAnimtionAmount(signal.Quantity);
        _defaultEndPosition = signal._endTransform;
        if (signal._startTransform != null)
            _defaultStartPosition = signal._startTransform;

        PlayAll();
        AudioController.PlaySFX(AudioType.ItemRattle);
        DOVirtual.DelayedCall(1, () =>
        {

            signal.powerUpButton.UpdateUI();
        });
    }

    private void ChangeObjectsSprites(Sprite sprite)
    {
        foreach (var item in _pool)
        {
            if (!item.gameObject.activeSelf)
                item.GetComponent<Image>().sprite = sprite;
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PowerUpPlayAnimationSignal>(PowerUpPlayAnimationSignal);
    }

    private Sprite GetPowerUpSprite(PowerupType powerupType)
    {
        for (int i = 0; i < _iconData.Length; i++)
        {
            if (_iconData[i].powerupType == powerupType) return _iconData[i].sprite;
        }
        return null;
    }

    [System.Serializable]
    public class IconData
    {
        public PowerupType powerupType;
        public Sprite sprite;
    }
}
public class PowerUpPlayAnimationSignal : ISignal
{
    public PowerupType powerupType;
    public int Quantity;
    public Transform _startTransform;
    public Transform _endTransform;
    public PowerUpButton powerUpButton;
}