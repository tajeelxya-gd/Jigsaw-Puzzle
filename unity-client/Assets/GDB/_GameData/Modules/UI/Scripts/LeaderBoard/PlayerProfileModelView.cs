using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileModelView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerIdTxt;
    [SerializeField] private TextMeshProUGUI mPlayerIdTxt;
    [SerializeField] private TextMeshProUGUI playerRankTxt;
    [SerializeField] private TextMeshProUGUI playerEarnedTrophiesTxt;
    [SerializeField] private Image playerPositionImg;
    [SerializeField] private Image playerPositionImgRoot;
    [SerializeField] private Sprite[] positionSprites;
    [SerializeField] private Image profileImg;
    [SerializeField] private Image bgImg;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Image _playerRankBg;
    [SerializeField] private Image _playerPointer;
    [SerializeField] private Color _playerTextColor;
    [SerializeField] private Color _otherTextColor;

    public void Init(bool showRankedBadges, string playerId, int listRanking, int playerRank, int playerEarnedTrophies, Sprite profilePicture, bool isPlayer = false)
    {
        playerIdTxt.text = playerId;
        mPlayerIdTxt.text = playerId;
        playerRankTxt.text = GetOrdinalText(playerRank);
        if (!showRankedBadges) playerRankTxt.gameObject.SetActive(true);
        else playerRankTxt.gameObject.SetActive(listRanking > positionSprites.Length - 1);
        _playerRankBg.enabled = playerRankTxt.gameObject.activeSelf;
        playerPositionImg.gameObject.SetActive(!(listRanking > positionSprites.Length - 1) && showRankedBadges);
        playerPositionImgRoot.transform.DOScale(!(listRanking > positionSprites.Length - 1) ? Vector3.one * 1.1f : Vector3.one, 0);
        playerPositionImg.sprite = positionSprites[Mathf.Clamp(listRanking, 0, positionSprites.Length - 1)];
        playerEarnedTrophiesTxt.text = playerEarnedTrophies.ToString();
        profileImg.sprite = profilePicture != null ? profilePicture : profileImg.sprite;
        bgImg.sprite = isPlayer ? playerSprite : defaultSprite;
        playerIdTxt.gameObject.SetActive(!isPlayer);
        mPlayerIdTxt.gameObject.SetActive(isPlayer);
        if (_playerPointer != null) _playerPointer.gameObject.SetActive(isPlayer);
        SetTextColors(isPlayer);
    }

    private void SetTextColors(bool isPlayer)
    {
        var color = isPlayer ? _playerTextColor : _otherTextColor;
        playerIdTxt.color = color;
        mPlayerIdTxt.color = color;
        playerEarnedTrophiesTxt.color = color;
    }


    public string GetOrdinalText(int number)
    {
        string suffix = "th";
        int lastDigit = number % 10;
        int lastTwoDigits = number % 100;

        if (lastTwoDigits < 11 || lastTwoDigits > 13)
        {
            switch (lastDigit)
            {
                case 1: suffix = "st"; break;
                case 2: suffix = "nd"; break;
                case 3: suffix = "rd"; break;
            }
        }

        return $"{number}<voffset=0.3em><size=60%>{suffix}</size></voffset>";
    }
}
