using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;
using UniTx.Runtime;

public class PlayerRanksModelViewController : MonoBehaviour
{
    [Title("Rank Settings")]
    [InfoBox("Rank list must contain exactly 3 elements!", InfoMessageType.Error, "IsListSizeInvalid")]
    [ListDrawerSettings(CustomAddFunction = null, HideAddButton = true, HideRemoveButton = true)]
    [SerializeField]
    private List<PlayerRankModelUI> ranks = new List<PlayerRankModelUI>() { null, null, null };

    private LeaderboardManagerRuntime _leaderBoardDataManager;
    private bool IsListSizeInvalid()
    {
        return ranks == null || ranks.Count != 3;
    }

    void OnPlayerRankUpdated()
    {

    }

    public void Inject(LeaderboardManagerRuntime leaderBoardDataManager)
    {
        _leaderBoardDataManager = leaderBoardDataManager;
        ProfileAvatarsData profileAvatarsData = leaderBoardDataManager.ProfileAvatarsData;
        for (int i = 0; i < ranks.Count; i++)
        {
            List<LeaderboardData.PlayerData> leagueTop30 = leaderBoardDataManager.LeagueTop30;
            ranks[i].SetUp(leagueTop30[i].PlayerName, profileAvatarsData.GetProfileAvatar(leagueTop30[i].PlayerAvatarID));
        }
        DOVirtual.DelayedCall(Time.deltaTime, LoadRanksRewardsData);
    }


    private IDataBase<LeaderBoardRanksRewardHolder> _rewardRanksDataService;

    private LeaderBoardRanksRewardHolder rewardsData = null;

    void LoadRanksRewardsData()
    {
        _rewardRanksDataService = new DataBaseService<LeaderBoardRanksRewardHolder>();
        rewardsData = _rewardRanksDataService.Load_Get();
        LeaderboardData.PlayerData playerProfileData = GetPlayerByID(_leaderBoardDataManager.HumanPlayerID, _leaderBoardDataManager.LeagueTop30);
        DOVirtual.DelayedCall(Time.deltaTime, () => LookForRewardsIfAvailable(playerProfileData));
    }

    public LeaderboardData.PlayerData GetPlayerByID(string playerID, List<LeaderboardData.PlayerData> leagueTop30)
    {
        return leagueTop30.Find(p => p.PlayerID == playerID);
    }

    void LookForRewardsIfAvailable(LeaderboardData.PlayerData playerData)
    {
        int currentPlayerRank = _leaderBoardDataManager.GetPlayerLeagueRank(playerData.PlayerID);
        if (currentPlayerRank <= 3 && currentPlayerRank >= 1)
        {
            UniStatics.LogInfo("Player Got On Top Spots :: " + currentPlayerRank);
            bool alreadyFound = false;
            foreach (var rewardData in rewardsData.ClaimedRewardsRanksList)
            {
                if (rewardData.Rank == currentPlayerRank && rewardData.LeagueType == playerData.CurrentLeague)
                {
                    alreadyFound = true;
                    UniStatics.LogInfo("Rank Reward Already Claimed!!!");
                    break;
                }
            }

            if (!alreadyFound)
            {
                LeaderBoardRewardRankData rewardRankData = new LeaderBoardRewardRankData();
                rewardRankData.Rank = currentPlayerRank;
                rewardRankData.LeagueType = playerData.CurrentLeague;
                rewardRankData.LastClaimedTime = DateTime.UtcNow.ToString("o");
                rewardsData.ClaimedRewardsRanksList.Add(rewardRankData);
                _rewardRanksDataService.Save(rewardsData);
                UniStatics.LogInfo("Rank RewardClaimed!!!");
                DOVirtual.DelayedCall(0.25f, () =>
                {
                    SignalBus.Publish(new OnDemandHomeScreenPanel { HomePageToOpen = 2, OnCompleteAction = () => OnClaimRankReward(currentPlayerRank) });
                    OnClaimRankReward(rewardRankData.Rank);
                });
            }
        }

    }

    void OnClaimRankReward(int rank)
    {
        Vector2 emitPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        GlobalService.IBulkPopService.PlayEffect(ranks[rank - 1].Reward, PopBulkService.BulkPopUpServiceType.Coins, emitPoint);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (ranks == null)
        {
            ranks = new List<PlayerRankModelUI>() { null, null, null };
        }
        else if (ranks.Count != 3)
        {
            while (ranks.Count < 3) ranks.Add(null);
            while (ranks.Count > 3) ranks.RemoveAt(ranks.Count - 1);
        }
    }
#endif




    [Serializable]
    public class LeaderBoardRanksRewardHolder
    {
        public List<LeaderBoardRewardRankData> ClaimedRewardsRanksList = new List<LeaderBoardRewardRankData>();
    }
    [Serializable]
    public class LeaderBoardRewardRankData
    {
        public int Rank = 0;
        public LeagueRankType LeagueType = LeagueRankType.Bronze;
        public string LastClaimedTime;
    }
}