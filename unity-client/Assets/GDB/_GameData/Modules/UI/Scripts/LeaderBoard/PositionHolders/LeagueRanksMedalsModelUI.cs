using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Added for Find

public class LeagueRanksMedalsModelUI : MonoBehaviour
{
    [SerializeField] private GameObject[] allLeaguesMedals;

    public void SetUp(List<LeaderboardData.PlayerData> leagueTop30)
    {
        //Debug.LogError("Updating Ranks ");
        var playerData = leagueTop30.FirstOrDefault(p => p.IsPlayer);

        if (playerData == null)
        {
            //Debug.LogError("PlayerData not found in the list!");
            DisableAll();
            return;
        }

        int currentLeagueIndex = (int)playerData.CurrentLeague;

        for (int i = 0; i < allLeaguesMedals.Length; i++)
        {
            bool shouldBeActive = i <= currentLeagueIndex;
            allLeaguesMedals[i].SetActive(shouldBeActive);
        }
    }

    private void DisableAll()
    {
        foreach (var medal in allLeaguesMedals) medal.SetActive(false);
    }
}