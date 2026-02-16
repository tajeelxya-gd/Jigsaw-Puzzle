using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GlobalLeagueController : LeagueCategoryBase
{
   [SerializeField] private LeaderboardManagerRuntime leaderBoardDataHandler;
   [SerializeField] private PlayersListLeagueModelView playerLeaguesListController;
   [SerializeField] private PlayerProfileModelView[] top3;



   public override void SetUpData()
   {
      UpdatePlayersListings();
   }

   void UpdatePlayersListings()
   {
      ProfileAvatarsData profileAvatarsData = leaderBoardDataHandler.ProfileAvatarsData;
      for (int i = 0; i < 3; i++)
      {
         LeaderboardData.PlayerData topPlayer = leaderBoardDataHandler.GlobalProgressiveTop30[i];
         top3[i].Init(true,topPlayer.PlayerName, i, topPlayer.PlayerRank, topPlayer.Trophies, profileAvatarsData.GetProfileAvatar(topPlayer.PlayerAvatarID), topPlayer.IsPlayer);
      }
      var remainingPlayers = leaderBoardDataHandler.GlobalProgressiveTop30.Skip(3).ToList();
      playerLeaguesListController.SetUp(remainingPlayers);

   }
}
