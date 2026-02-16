using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

public class LeaderboardJSONGenerator : EditorWindow
{
    public int numberOfPlayers = 100;
    public string saveFileName = "InitialLeaderboard.json";

    public List<string> uniqueNames = new List<string> { "John", "Alice", "Max", "Luna", "Hash324", "Nova", "Zyra", "Leo", "Eva", "Kai" };

    [MenuItem("Tools/Leaderboard JSON Generator")]
    public static void ShowWindow()
    {
        GetWindow<LeaderboardJSONGenerator>("Leaderboard JSON Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Leaderboard JSON", EditorStyles.boldLabel);

        numberOfPlayers = EditorGUILayout.IntField("Number of Players", numberOfPlayers);
        saveFileName = EditorGUILayout.TextField("File Name", saveFileName);

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty uniqueNamesProp = serializedObject.FindProperty("uniqueNames");
        EditorGUILayout.PropertyField(uniqueNamesProp, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Generate JSON"))
        {
            GenerateJSON();
        }
    }

    void GenerateJSON()
    {
        LeaderboardData manager = new LeaderboardData();
        HashSet<string> usedNames = new HashSet<string>();
        System.Random rnd = new System.Random();
        string[] activityLevels = { "VeryActive", "Regular", "Casual", "Rare" };

        // League distribution
        float bronzePercent = 0.4f;
        float silverPercent = 0.3f;
        float goldPercent = 0.2f;
        float diamondPercent = 0.1f;

        int bronzeCount = Mathf.RoundToInt(numberOfPlayers * bronzePercent);
        int silverCount = Mathf.RoundToInt(numberOfPlayers * silverPercent);
        int goldCount = Mathf.RoundToInt(numberOfPlayers * goldPercent);
        int diamondCount = numberOfPlayers - bronzeCount - silverCount - goldCount;

        List<LeagueRankType> leaguePool = new List<LeagueRankType>();
        leaguePool.AddRange(System.Linq.Enumerable.Repeat(LeagueRankType.Bronze, bronzeCount));
        leaguePool.AddRange(System.Linq.Enumerable.Repeat(LeagueRankType.Silver, silverCount));
        leaguePool.AddRange(System.Linq.Enumerable.Repeat(LeagueRankType.Gold, goldCount));
        leaguePool.AddRange(System.Linq.Enumerable.Repeat(LeagueRankType.Diamond, diamondCount));

        // Shuffle league pool
        for (int i = 0; i < leaguePool.Count; i++)
        {
            int swapIndex = rnd.Next(i, leaguePool.Count);
            (leaguePool[i], leaguePool[swapIndex]) = (leaguePool[swapIndex], leaguePool[i]);
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            LeaderboardData.PlayerData player = new LeaderboardData.PlayerData();
            player.PlayerID = "AI" + (i + 1).ToString("D3");

            // Name assignment
            if (rnd.NextDouble() < 0.8) // 80% Player####
            {
                string name;
                do { name = "Player#" + rnd.Next(1000, 9999); } while (usedNames.Contains(name));
                player.PlayerName = name;
                usedNames.Add(name);
            }
            else // 20% unique
            {
                if (uniqueNames.Count == 0)
                {
                    string name;
                    do { name = "Player#" + rnd.Next(1000, 9999); } while (usedNames.Contains(name));
                    player.PlayerName = name;
                    usedNames.Add(name);
                }
                else
                {
                    string name;
                    do { name = uniqueNames[rnd.Next(uniqueNames.Count)] + rnd.Next(100, 999); } while (usedNames.Contains(name));
                    player.PlayerName = name;
                    usedNames.Add(name);
                }
            }

            // Assign league from shuffled pool
            LeagueRankType assignedLeague = leaguePool[i];
            player.CurrentLeague = assignedLeague;

            // Assign trophies based on league (1–250 range)
          
            LeagueConfig config = LeagueConfigManager.GetConfig(assignedLeague);
            player.Trophies = rnd.Next(config.MinTrophies, config.MaxTrophies + 1);
            // Streak
            player.Streak = rnd.Next(0, 6);

            // Base gain
            player.BaseGainPerLevel = rnd.Next(1, 3);

            // Volatility
            player.Volatility = (float)Math.Round(rnd.NextDouble() * 0.2 + 0.1, 2);

            // Activity
            player.ActivityLevel = activityLevels[rnd.Next(activityLevels.Length)];

            // Decay
            switch (assignedLeague)
            {
                case LeagueRankType.Bronze: player.DecayRate = rnd.Next(0, 6); break;
                case LeagueRankType.Silver: player.DecayRate = rnd.Next(5, 11); break;
                case LeagueRankType.Gold: player.DecayRate = rnd.Next(10, 16); break;
                case LeagueRankType.Diamond: player.DecayRate = rnd.Next(15, 21); break;
            }

            // AvatarID 0–8
            player.PlayerAvatarID = rnd.Next(0, 9);

            // Last simulated
            player.LastSimulatedTime = DateTime.UtcNow.ToString("o");

            UnityEngine.Debug.Log("Player: " + player.PlayerName + " Rank : " + player.CurrentLeague + " Trohpies : " + player.Trophies);
            manager.AllPlayers.Add(player);
        }

        // Save
        string folderPath = "Assets/Resources";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string path = Path.Combine(folderPath, saveFileName);
        string json = JsonUtility.ToJson(manager, true);
        File.WriteAllText(path, json);

        AssetDatabase.Refresh();
        Debug.Log($"Generated {numberOfPlayers} players with enums, avatars, and trophy ranges 1-250 at {path}");
    }

  
}
