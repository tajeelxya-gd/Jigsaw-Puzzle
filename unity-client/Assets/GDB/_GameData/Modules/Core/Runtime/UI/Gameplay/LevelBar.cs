using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelLabel;
    [SerializeField] private Image _tagImg;
    [SerializeField] private Sprite _hardImgTag, _superHardImgTag;

    public void Initialize(LevelType levelType)
    {
        _levelLabel.text = "Lvl " + GlobalService.GameData.Data.LevelNumber;

        _tagImg.gameObject.SetActive(levelType != LevelType.Easy);

        if (levelType == LevelType.Hard)
            _tagImg.sprite = _hardImgTag;
        else if (levelType == LevelType.SuperHard)
            _tagImg.sprite = _superHardImgTag;
    }
}