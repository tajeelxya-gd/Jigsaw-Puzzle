using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GoldBundle : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] private Sprite _icon;
     [SerializeField] GoldBundlesData _goldBundlesData;
    [SerializeField] private TextMeshProUGUI[] rewardsText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        SetUp();
    }

    void SetUp()
    {
        foreach (var rewardtxt in rewardsText)
        {
           
            rewardtxt.text = _goldBundlesData.GoldReward.rewardChestAmount
                .ToString("N0", CultureInfo.InvariantCulture);
        }
    }

    private void OnValidate()
    {
        if(_image && _icon)
            _image.sprite = _icon;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
