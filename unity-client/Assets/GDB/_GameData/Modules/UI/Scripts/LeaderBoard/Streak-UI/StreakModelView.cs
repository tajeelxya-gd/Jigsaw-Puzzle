using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StreakModelView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Image imageComponent;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite unSelectedSprite;
    [SerializeField] private int rewardAmount = 1;
    public int RewardMultiplier => rewardAmount;
    Vector2 _defaultSizeDelta = Vector2.one;
    private int _defaultSiblingIndex = 0;
    private void Awake()
    {
        _defaultSizeDelta = imageComponent.GetComponent<RectTransform>().sizeDelta;
        _defaultSiblingIndex =  transform.GetSiblingIndex();
    }

    private void OnValidate()
    {
        rewardText.text = "x"+rewardAmount.ToString();
    }

    public void OnSelected(bool selected)
    {
        imageComponent.sprite = selected ? selectedSprite : unSelectedSprite;
        imageComponent.transform.localScale = Vector3.one * (selected ? 1.5f : 1f); 
        if (selected) transform.SetAsLastSibling();
        else transform.SetSiblingIndex(_defaultSiblingIndex);
    }
}
