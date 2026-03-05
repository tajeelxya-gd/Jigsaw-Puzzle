using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HolderManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _number;
    [SerializeField] private TextMeshProUGUI _number_special;
    [SerializeField] private Image _icon;
    [SerializeField] private Image number_icon;
    [SerializeField] private Sprite _puzzleSprite;
    [SerializeField] private CanvasGroup _icon_FadeGroup;
    [SerializeField] private GameObject _collectedImage;
    [SerializeField] private Image _holderImage;
    [SerializeField] private Sprite _dimHolder;
    [SerializeField] private Sprite _originalSprite;
    [SerializeField] private RewardContainerUI _rewardContainer;
    public void SetupHolder(int number, Sprite icon, int rewardAmount)
    {
        _number.text = number.ToString();
        _number_special.text = number.ToString();
        _icon.sprite = icon;
        _rewardContainer.Init(icon, rewardAmount);
        _number.gameObject.SetActive(number_icon.sprite != _puzzleSprite);
        _number_special.gameObject.SetActive(number_icon.sprite == _puzzleSprite);
    }

    public void SetHolderImage(Sprite img)
    {
        _holderImage.sprite = img;
        _icon.gameObject.SetActive(true);
        _holderImage.color = Color.white;
        _icon.color = Color.white;
        _icon_FadeGroup.alpha = 1f;
    }

    public void ShowCollectedIcon()
    {
        // _number.gameObject.SetActive(false);
        _collectedImage.SetActive(true);
    }

    public void DimRewardIcon()
    {
        _icon_FadeGroup.DOFade(0.25f, 0.5f);
        _holderImage.sprite = _dimHolder;
        _icon.gameObject.SetActive(false);
    }

    public void SetLocked(bool locked)
    {
        _icon_FadeGroup.alpha = locked ? 0.4f : 1f;
        _holderImage.sprite = locked ? _dimHolder : _originalSprite;
    }

    public RectTransform GetTransform()
    {
        return (RectTransform)transform;
    }

    public void Reset()
    {
        _number.gameObject.SetActive(true);
        _collectedImage.SetActive(false);
        _icon_FadeGroup.DOFade(1f, 0.5f);
        _holderImage.sprite = _originalSprite;
    }
}