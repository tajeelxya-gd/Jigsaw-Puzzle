// using UnityEngine;
//
// public class OnGetReward : MonoBehaviour
// {
//     [SerializeField] private Item[] _rewardItem;
//     [SerializeField] private RewardCard _rewardCard;
//     
//     
//     
//
//     public void GiveReward()
//     {
//         _rewardItem = _rewardCard.GetRewards();
//
//         if (_rewardItem == null || _rewardItem.Length == 0) return;
//
//         for (int i = 0; i < _rewardItem.Length; i++)
//         {
//             //REWARD LOGIC HERE
//         }
//
//         _rewardCard.MarkAsCollected();
//         DisableButton();
//     }
//     
// }