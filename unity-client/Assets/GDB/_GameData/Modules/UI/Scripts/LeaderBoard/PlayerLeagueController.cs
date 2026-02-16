using System;
using DG.Tweening;
using UnityEngine;

public class PlayerLeagueController : LeagueCategoryBase
{
   [SerializeField] private LeaderboardManagerRuntime leaderBoardDataHandler;
   [SerializeField] private PlayerRanksModelViewController playerRanksModelViewController;
   [SerializeField] private StreakModelViewController streaksModelViewController;
   [SerializeField] private PlayersListLeagueModelView playerLeaguesListController;
   [SerializeField] private LeagueRanksMedalsModelUI playerLeagueRanksMedalsModelUI;

  public override void SetUpData()
   {
      UpdatePlayersListings();
      UpdateMedalLeaguesRanks();
      UpdateStreaksStatus();
      UpdatePositioningRanks();
   }

   void UpdatePlayersListings()
   {
      playerLeaguesListController.SetUp(leaderBoardDataHandler.LeagueTop30);
   }
   void UpdatePositioningRanks()
   {
      playerRanksModelViewController.Inject(leaderBoardDataHandler);
   }
   void UpdateMedalLeaguesRanks()
   {
      playerLeagueRanksMedalsModelUI.SetUp(leaderBoardDataHandler.LeagueTop30);
   }

   void UpdateStreaksStatus()
   {
      
   }
}
