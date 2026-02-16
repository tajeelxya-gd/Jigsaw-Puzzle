using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerRankModelUI : MonoBehaviour
{
 [SerializeField] TextMeshProUGUI playerNameText;
 [SerializeField] TextMeshProUGUI playerNameShadowText;
 [SerializeField] private Image playerProfileImg;
 [SerializeField] private TextMeshProUGUI rewardTxt;
 [SerializeField] private TextMeshProUGUI rewardTxt_shadowText;
 [SerializeField] private int reward = 75;
 public int Reward => reward;
 public void SetUp(string playerName, Sprite playerProfilePic)
   {
    playerNameText.text = playerName;
    playerNameShadowText.text = playerName;
    playerProfileImg.sprite = playerProfilePic;
    rewardTxt.text = reward.ToString();
    rewardTxt_shadowText.text = reward.ToString();
   }   
   
}
