using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;


public enum LeagueRankType { Bronze, Silver, Gold, Diamond }
[Serializable]
public class LeaderboardData
{
    [Serializable]
    public class PlayerData
    {
        public string PlayerID;
        public string PlayerName;
        public int Trophies;
        public LeagueRankType CurrentLeague;
        public int Streak;
        public int BaseGainPerLevel;
        public float Volatility;
        public string ActivityLevel;
        public int DecayRate;
        public string LastSimulatedTime;
        public int PlayerRank = 0;
        public int PlayerAvatarID = 0;

        [Header("Is this the human player?")]
        public bool IsPlayer = false;
    }

    public List<PlayerData> AllPlayers = new List<PlayerData>();
}

public class LeaderboardManagerRuntime : MonoBehaviour
{
    [Header("Leaderboard Data")]
    public LeaderboardData leaderboardData;
    [SerializeField] LeagueCategoryBase leagueControllerUI;
    [SerializeField] LeagueCategoryBase globalLeagueControllerUI;
    [SerializeField] ProfileAvatarsData profileAvatarsData;
    public ProfileAvatarsData  ProfileAvatarsData => profileAvatarsData;
    [SerializeField] private List<LeaderboardData.PlayerData> globalTop30 = new List<LeaderboardData.PlayerData>();
    [SerializeField] private List<LeaderboardData.PlayerData> leagueTop30 = new List<LeaderboardData.PlayerData>();
    public List<LeaderboardData.PlayerData> GlobalTop30 => globalTop30;
    [SerializeField] private List<LeaderboardData.PlayerData> globalProgressiveTop30 = new List<LeaderboardData.PlayerData>();
    public List<LeaderboardData.PlayerData> GlobalProgressiveTop30 => globalProgressiveTop30;
    [ReadOnly,SerializeField] public List<LeaderboardData.PlayerData> LeagueTop30 => leagueTop30;
    [ReadOnly,SerializeField] private List<LeaderboardData.PlayerData> leagueBronze = new List<LeaderboardData.PlayerData>();
    [ReadOnly,SerializeField] private List<LeaderboardData.PlayerData> leagueSilver = new List<LeaderboardData.PlayerData>();
    [ReadOnly,SerializeField] private List<LeaderboardData.PlayerData> leagueGold = new List<LeaderboardData.PlayerData>();
    [ReadOnly,SerializeField] private List<LeaderboardData.PlayerData> leagueDiamond = new List<LeaderboardData.PlayerData>();
    
    [Header("JSON file in Resources (no extension)")]
    [SerializeField]
    public string jsonFileName = "InitialLeaderboard";

    [Header("Human player settings")]
    [SerializeField]
    string humanPlayerID = "USER001";
    public string HumanPlayerID => humanPlayerID;
    [SerializeField]
    string humanPlayerName = "MyPlayer";
    public string HumanPlayerName => humanPlayerName;
    [SerializeField]
    int humanPlayerTrophies = 0;
    [SerializeField]
    LeagueRankType humanPlayerLeague = LeagueRankType.Bronze;

    private string savePath;

    public string GetPlayerID() => humanPlayerID;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "LeaderboardSave.json");
        LoadLeaderboard();
        AddHumanPlayer();
        SimulateOfflineAI();
        SaveLeaderboard();
       DOVirtual.DelayedCall(0.5f, UpdatePlayerStats);
    }

    private void Start()
    {
        SignalBus.Subscribe<OnPlayerNameChange>(UpdateMyProfileName);
    }

    void UpdatePlayerStats()
    {
        GameData _gameData = GlobalService.GameData;
        if (_gameData.Data.TrophiesWinInGame > 0)
            PlayerWinLevel(humanPlayerID);
    }

    public void ResetAllData()
    {
        
    }

    #region Load / Save
    public void LoadLeaderboard()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
            //Debug.Log("Leaderboard loaded from persistent save.");
        }
        else
        {
            LoadDefaultUsersData();
        }
        UpdateGlobalTop30();
    }

    public void LoadDefaultUsersData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);
        if (jsonFile != null)
        {
            leaderboardData = JsonUtility.FromJson<LeaderboardData>(jsonFile.text);
           // Debug.Log($"Leaderboard loaded from Resources: {leaderboardData.AllPlayers.Count} AI players.");
        }
        else
        {
          //  Debug.LogWarning($"Leaderboard JSON '{jsonFileName}' not found. Creating empty leaderboard.");
            leaderboardData = new LeaderboardData();
        }
    }

    public void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(leaderboardData, true);
        File.WriteAllText(savePath, json);
       // Debug.Log("Leaderboard saved to persistentDataPath.");
    }
    #endregion

    #region Human Player
    private DataBaseService<PlayerProfileData> _profileDataBase;
    private PlayerProfileData _playerProfileData;

    void UpdateMyProfileName(ISignal signal = null)
    {
        LeaderboardData.PlayerData lbProfileData = GetPlayerByID(humanPlayerID);
        _profileDataBase = new DataBaseService<PlayerProfileData>();
        _playerProfileData = _profileDataBase.Load_Get();
        lbProfileData.PlayerName = _playerProfileData._playerName;
        SaveLeaderboard();
    }
    public void AddHumanPlayer()
    {
        var existing = GetPlayerByID(humanPlayerID);
        if (existing != null)
        {
            UpdateMyProfileName();
            existing.IsPlayer = true;
            return;
        }

        LeaderboardData.PlayerData myProfile = new LeaderboardData.PlayerData
        {
            PlayerID = humanPlayerID,
            PlayerName = humanPlayerName,
            Trophies = humanPlayerTrophies,
            CurrentLeague = humanPlayerLeague,
            Streak = 0,
            BaseGainPerLevel = 1,
            Volatility = 0.1f,
            ActivityLevel = "Human",
            DecayRate = 0,
            LastSimulatedTime = DateTime.UtcNow.ToString("o"),
            IsPlayer = true
        };
      
        leaderboardData.AllPlayers.Add(myProfile);
        UpdateGlobalTop30();
        UpdateLeagueTop30(myProfile.CurrentLeague);
        UpdateMyProfileName();
        DOVirtual.DelayedCall(0.1f, ()=>UpdateMyProfileName());
    }

    void UpdateRewardStatusOfPlayer()
    {
        
    }

    #endregion

    #region Leaderboard Updates
    public void UpdateGlobalTop30()
    {
        List<LeaderboardData.PlayerData> sortedPlayerData = new List<LeaderboardData.PlayerData>(leaderboardData.AllPlayers);
        sortedPlayerData.Sort((a, b) => b.Trophies.CompareTo(a.Trophies));
        for (int i = 0; i < sortedPlayerData.Count; i++)
        {
            sortedPlayerData[i].PlayerRank =  i;    
        }
        globalTop30 = new List<LeaderboardData.PlayerData>(leaderboardData.AllPlayers);
        globalTop30.Sort((a, b) => b.Trophies.CompareTo(a.Trophies));
        if (globalTop30.Count > 30)
            globalTop30 = globalTop30.GetRange(0, 30);

        UpdateAllLeagues();
        UpdateGlobalProgressiveTop30();
    }
    
    public void UpdateGlobalProgressiveTop30()
    {
        if (leaderboardData == null || leaderboardData.AllPlayers == null || leaderboardData.AllPlayers.Count == 0)
            return;

        var sorted = new List<LeaderboardData.PlayerData>(leaderboardData.AllPlayers);
        sorted.Sort((a, b) => b.Trophies.CompareTo(a.Trophies));

        var human = GetPlayerByID(humanPlayerID);

        if (human == null)
            return;

        int humanIndex = sorted.FindIndex(p => p.PlayerID == humanPlayerID);

        globalProgressiveTop30 = new List<LeaderboardData.PlayerData>();

        // 🔒 Always include TRUE global top-3
        var top3 = sorted.Take(3).ToList();
        globalProgressiveTop30.AddRange(top3);

        // remaining slots after top3
        int remainingSlots = 30 - globalProgressiveTop30.Count;

        // If human is already in top3 → just fill normally
        if (humanIndex < 3)
        {
            var rest = sorted.Skip(3).Take(remainingSlots).ToList();
            globalProgressiveTop30.AddRange(rest);
            return;
        }

        // 📌 Build contextual window around human
        int start = Mathf.Clamp(
            humanIndex - (remainingSlots / 2),
            3,                                         // never under top 3
            Mathf.Max(3, sorted.Count - remainingSlots)
        );

        var window = sorted.Skip(start).Take(remainingSlots).ToList();
        globalProgressiveTop30.AddRange(window);

        // guarantee human included
        if (!globalProgressiveTop30.Any(p => p.PlayerID == humanPlayerID))
        {
            globalProgressiveTop30[globalProgressiveTop30.Count - 1] = human;
        }
        
        globalProgressiveTop30.Sort((a, b) => b.Trophies.CompareTo(a.Trophies));
        UpdateAllLeagues();
    }


    void UpdateAllLeagues()
    {
        leagueBronze = leaderboardData.AllPlayers.FindAll(p => p.CurrentLeague.Equals(LeagueRankType.Bronze));
        leagueSilver = leaderboardData.AllPlayers.FindAll(p => p.CurrentLeague.Equals(LeagueRankType.Silver));
        leagueGold = leaderboardData.AllPlayers.FindAll(p => p.CurrentLeague.Equals(LeagueRankType.Gold));
        leagueDiamond = leaderboardData.AllPlayers.FindAll(p => p.CurrentLeague.Equals(LeagueRankType.Diamond));
    }
public void UpdateLeagueTop30(LeagueRankType league)
{
    List<LeaderboardData.PlayerData> leaguePlayers = leaderboardData.AllPlayers
        .FindAll(p => p.CurrentLeague.Equals(league));
    leaguePlayers.Sort((a, b) => b.Trophies.CompareTo(a.Trophies));

    int humanIndex = leaguePlayers.FindIndex(p => p.PlayerID == humanPlayerID);
    if (humanIndex == -1) humanIndex = 0; // fallback if not found

    List<LeaderboardData.PlayerData> newTop30 = new List<LeaderboardData.PlayerData>();

    int maxListCount = Math.Min(30, leaguePlayers.Count);

    // 1️⃣ Always add top 3 players
    int topCount = Math.Min(3, leaguePlayers.Count);
    for (int i = 0; i < topCount; i++)
        newTop30.Add(leaguePlayers[i]);

    int remainingSlots = maxListCount - newTop30.Count;

    // 2️⃣ Decide sliding window around human
    int windowSize = remainingSlots - 1; // minus 1 for human
    int halfWindow = windowSize / 2;

    int start = humanIndex - halfWindow;
    int end = humanIndex + (windowSize - halfWindow);

    // Clamp to league bounds
    start = Mathf.Max(3, start); // never overwrite top 3
    end = Mathf.Min(leaguePlayers.Count - 1, end);

    // Adjust start/end if we have fewer than remainingSlots
    int actualCount = end - start + 1;
    if (actualCount < windowSize)
    {
        // try to extend start backwards
        start = Mathf.Max(3, start - (windowSize - actualCount));
        actualCount = end - start + 1;
    }
    if (actualCount < windowSize)
    {
        // try to extend end forward
        end = Mathf.Min(leaguePlayers.Count - 1, end + (windowSize - actualCount));
    }

    // 3️⃣ Add players above/below human (window)
    for (int i = start; i <= end; i++)
    {
        if (!newTop30.Contains(leaguePlayers[i]))
            newTop30.Add(leaguePlayers[i]);
    }

    // 4️⃣ Add human explicitly if not in list (rare case)
    if (!newTop30.Contains(leaguePlayers[humanIndex]))
        newTop30.Add(leaguePlayers[humanIndex]);

    // 5️⃣ Trim to max 30 just in case
    if (newTop30.Count > maxListCount)
        newTop30 = newTop30.GetRange(0, maxListCount);

    leagueTop30 = newTop30;
}



    #endregion

    #region Player Utilities
    public LeaderboardData.PlayerData GetPlayerByID(string playerID)
    {
        return leaderboardData.AllPlayers.Find(p => p.PlayerID == playerID);
    }
    public void AddTrophiesToPlayer(string playerID, int trophies)
    {
        var player = GetPlayerByID(playerID);
        if (player != null)
        {
            player.Trophies += trophies;
            player.LastSimulatedTime = DateTime.UtcNow.ToString("o");
            CheckLeaguePromotion(player);
            UpdateGlobalTop30();
            UpdateLeagueTop30(player.CurrentLeague);
            SaveLeaderboard();
        }
    }

    // ✅ Correct global rank even if player is not top 30
    public int GetPlayerGlobalRankFull(string playerID)
    {
        List<LeaderboardData.PlayerData> sorted = new List<LeaderboardData.PlayerData>(leaderboardData.AllPlayers);
        sorted.Sort((a, b) => b.Trophies.CompareTo(a.Trophies));
        int index = sorted.FindIndex(p => p.PlayerID == playerID);
        return index + 1;
    }

    public int GetPlayerLeagueRank(string playerID)
    {
        return leagueTop30.FindIndex(p => p.PlayerID == playerID) + 1;
    }
    #endregion

    #region Trophy & Streak Logic

    public void PlayerWinLevel(string playerID, bool isTest = false)
    {
        var player = GetPlayerByID(playerID);
        if (player == null) return;

        int[] multipliers = { 1, 2, 3, 5, 10 };
        int streakIndex = Mathf.Clamp(player.IsPlayer ? 0 : player.Streak, 0, multipliers.Length - 1);
       // Debug.LogError(player.BaseGainPerLevel+"  "+ multipliers[streakIndex] + "" +streakIndex);
        int gain = player.BaseGainPerLevel * multipliers[streakIndex];
        GameData gameData =  GlobalService.GameData;
        gameData.Data.TrophiesWinInGame = isTest ? gain : gameData.Data.TrophiesWinInGame;
        player.Trophies += gameData.Data.TrophiesWinInGame;
        gameData.Data.TrophiesWinInGame = 0;
        gameData.Save();
        if(player.IsPlayer)
            player.Streak = 1;
        else player.Streak += 1;
        player.LastSimulatedTime = DateTime.UtcNow.ToString("o");

        CheckLeaguePromotion(player);
        UpdateGlobalTop30();
        UpdateLeagueTop30(player.CurrentLeague);
        SaveLeaderboard();
        UpdateModelView(Time.deltaTime);
       // Debug.Log($"{player.PlayerName} won a level: +{gain} trophies, streak {player.Streak}");
    }

    void UpdateModelView(float delay = 0.02f)
    {
        DOVirtual.DelayedCall(Time.deltaTime, () => { leagueControllerUI.SetUpData(); });
        DOVirtual.DelayedCall(Time.deltaTime, () => { globalLeagueControllerUI.SetUpData(); });
    }
    public void PlayerLoseLevel(string playerID)
    {
        var player = GetPlayerByID(playerID);
        if (player == null) return;

        player.Streak = 0;
        int loss = Mathf.Min(player.Trophies, player.BaseGainPerLevel);
        player.Trophies -= loss;
        CheckLeaguePromotion(player);
        UpdateGlobalTop30();
        UpdateLeagueTop30(player.CurrentLeague);
        SaveLeaderboard();
        UpdateModelView(Time.deltaTime);

        //Debug.Log($"{player.PlayerName} lost a level: -{loss} trophies, streak reset");
    }

    private void CheckLeaguePromotion(LeaderboardData.PlayerData player)
    {
        player.CurrentLeague = LeagueConfigManager.GetLeagueByTrophies(player.Trophies);
    }
    #endregion

    #region Hybrid AI Simulation
    public void SimulateOfflineAI()
    {
        System.Random rnd = new System.Random();
        DateTime now = DateTime.UtcNow;

        // store old ranks & trophies for logging
        Dictionary<string, (int trophies, int rank)> before =
            new Dictionary<string, (int, int)>();

        foreach (var p in leaderboardData.AllPlayers)
            before[p.PlayerID] = (p.Trophies, GetPlayerGlobalRankFull(p.PlayerID));

        List<LeaderboardData.PlayerData> playersChanged = new List<LeaderboardData.PlayerData>();

            foreach (var ai in leaderboardData.AllPlayers)
            {
                if (ai.IsPlayer) continue;

                DateTime lastSim = DateTime.Parse(ai.LastSimulatedTime);
                double hoursPassed = (now - lastSim).TotalHours;

                if (hoursPassed < 0.01)
                    hoursPassed = 1;   // make sure something can happen in tests

                int gain = ComputeAIGain(ai, hoursPassed, rnd);

                if (gain != 0)
                {
                    ai.Trophies += gain;
                    ai.LastSimulatedTime = now.ToString("o");
                    playersChanged.Add(ai);
                }

                // streak behavior
                if (rnd.NextDouble() < 0.1)
                    ai.Streak = 0;
                else
                    ai.Streak = Mathf.Clamp(ai.Streak + 1, 0, 5);

                CheckLeaguePromotion(ai);
            }

        UpdateGlobalTop30();
        UpdateLeagueTop30(GetPlayerByID(humanPlayerID).CurrentLeague);
        SaveLeaderboard();
        UpdateModelView(Time.deltaTime);

        //Debug.Log($"Offline AI Simulation complete: {playersChanged.Count} AI updated");

        // 🔥 detailed change log
        foreach (var ai in playersChanged)
        {
            var beforeData = before[ai.PlayerID];
            int newRank = GetPlayerGlobalRankFull(ai.PlayerID);

           // Debug.Log(
            //     $"AI Updated: {ai.PlayerName} | " +
            //     $"Trophies: {beforeData.trophies} → {ai.Trophies} | " +
            //     $"Rank: {beforeData.rank} → {newRank} | " +
            //     $"League: {ai.CurrentLeague}"
            // );
        }
    }


    public int ComputeAIGain(LeaderboardData.PlayerData ai, double hoursPassed, System.Random rnd)
    {
        double activityFactor = ai.ActivityLevel switch
        {
            "VeryActive" => 1.0,
            "Regular"    => 0.7,
            "Casual"     => 0.5,
            "Rare"       => 0.2,
            _            => 0.3
        };

        double baseGain = ai.BaseGainPerLevel;
        double vol = ai.Volatility;

        // 📈 Gain calculation
        double rawGain =
            baseGain *
            (1 + (rnd.NextDouble() - 0.5) * vol) *
            activityFactor *
            (hoursPassed / 2.0);

        // 📉 Decay calculation
        double rawDecay =
            ai.DecayRate *
            (hoursPassed / 24.0);

        double finalDelta = rawGain - rawDecay;

        // 🛟 Anti-stall rule
        if (finalDelta > 0 && finalDelta < 1)
            finalDelta = 1;

        return Mathf.Max(0, Mathf.RoundToInt((float)finalDelta));
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnPlayerNameChange>(UpdateMyProfileName);

    }

    #endregion
    
}

