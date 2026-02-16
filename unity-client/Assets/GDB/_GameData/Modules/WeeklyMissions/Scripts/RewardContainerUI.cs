using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class RewardContainerUI : MonoBehaviour
{
    [SerializeField] Image _itemIcon;
    [SerializeField] TextMeshProUGUI _itemRewardAmount;
    [SerializeField] TextMeshProUGUI _itemReward_ShadowText;
    [SerializeField] bool _destroyOnDisable = true;
    public void Init(Sprite itemSprie, int itemReward)
    {
        _itemIcon.sprite = itemSprie;
        _itemRewardAmount.text = itemReward.ToString();
        if(_itemReward_ShadowText)
            _itemReward_ShadowText.text = itemReward.ToString();
    }

    private void OnDisable()
    {
        if (_destroyOnDisable)
            Destroy(gameObject);
    }
}
