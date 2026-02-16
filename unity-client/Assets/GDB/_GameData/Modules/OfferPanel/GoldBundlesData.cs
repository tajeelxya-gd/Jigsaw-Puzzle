using UnityEngine;
[CreateAssetMenu(fileName = "Gold Bundle", menuName = "Scriptable Objects/GolD Bundles")]

public class GoldBundlesData : ScriptableObject
{
  [SerializeField] private RewardProgressModelView _goldReward;
  public RewardProgressModelView GoldReward => _goldReward;
}
 