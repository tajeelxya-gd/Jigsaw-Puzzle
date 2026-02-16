#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardTest : MonoBehaviour
{
    [Header("Reference to Leaderboard Manager")]
    public LeaderboardManagerRuntime leaderboardManager;

    [Header("Human Player Login Simulation")]
    public string humanPlayerID = "USER001";
    public int humanTrophiesGain = 50;

#if ODIN_INSPECTOR
    [Button("Simulate Login")]
#endif
    public void SimulateLogin()
    {
        if (leaderboardManager == null)
        {
            Debug.LogError("LeaderboardManagerRuntime is not assigned!");
            return;
        }

        Debug.Log("=== Simulating Player Login ===");

        // Store old ranks
        Dictionary<string, int> oldRanks = new Dictionary<string, int>();
        foreach (var p in leaderboardManager.leaderboardData.AllPlayers)
            oldRanks[p.PlayerID] = leaderboardManager.GetPlayerGlobalRankFull(p.PlayerID);

        // Update human player trophies
        var humanPlayer = leaderboardManager.GetPlayerByID(humanPlayerID);
        if (humanPlayer != null)
        {
          //  humanPlayer.Trophies += humanTrophiesGain;
          //  humanPlayer.LastSimulatedTime = DateTime.UtcNow.ToString("o");
        }

        // Simulate AI
        leaderboardManager.SimulateOfflineAI();

        // Update leaderboards
        leaderboardManager.UpdateGlobalTop30();
        leaderboardManager.UpdateLeagueTop30(humanPlayer.CurrentLeague);

        // Log players whose ranks changed
        Debug.Log("=== Players whose ranks changed ===");
        foreach (var p in leaderboardManager.leaderboardData.AllPlayers)
        {
            int newRank = leaderboardManager.GetPlayerGlobalRankFull(p.PlayerID);
            if (oldRanks.TryGetValue(p.PlayerID, out int oldRank) && newRank != oldRank)
                Debug.Log($"Player '{p.PlayerName}' rank changed: {oldRank} → {newRank} (Trophies: {p.Trophies})");
        }

        // Log all leagues
        Debug.Log("=== League Status ===");
        foreach (var p in leaderboardManager.leaderboardData.AllPlayers)
        {
            string flag = p.IsPlayer ? "(Human)" : "";
            Debug.Log($"Player '{p.PlayerName}' {flag} - League: {p.CurrentLeague}, Trophies: {p.Trophies}");
        }

        // ✅ Human player rank & league
        int humanGlobalRank = leaderboardManager.GetPlayerGlobalRankFull(humanPlayerID);
        Debug.Log($"*** HUMAN PLAYER STATUS ***: Rank: {humanGlobalRank}, League: {humanPlayer.CurrentLeague}, Trophies: {humanPlayer.Trophies}");

        leaderboardManager.SaveLeaderboard();

        Debug.Log("=== Login Simulation Complete ===");
    }

#if ODIN_INSPECTOR
    [Button("Simulate Level Win")]
#endif
    public void SimulateLevelWin()
    {
        leaderboardManager.PlayerWinLevel(humanPlayerID,true);
        var human = leaderboardManager.GetPlayerByID(humanPlayerID);
        int rank = leaderboardManager.GetPlayerGlobalRankFull(humanPlayerID);
        Debug.Log($"*** HUMAN PLAYER AFTER WIN ***: Rank: {rank}, League: {human.CurrentLeague}, Trophies: {human.Trophies}");
    }

#if ODIN_INSPECTOR
    [Button("Simulate Level Lose")]
#endif
    public void SimulateLevelLose()
    {
        leaderboardManager.PlayerLoseLevel(humanPlayerID);
        var human = leaderboardManager.GetPlayerByID(humanPlayerID);
        int rank = leaderboardManager.GetPlayerGlobalRankFull(humanPlayerID);
        Debug.Log($"*** HUMAN PLAYER AFTER LOSE ***: Rank: {rank}, League: {human.CurrentLeague}, Trophies: {human.Trophies}");
    }
}
