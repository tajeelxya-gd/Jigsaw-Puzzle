using System;
using System.Collections.Generic;

public class GameData
{
    private GData _data;
    public GData Data => _data;

    private IDataBase<GData> _dataBase;

    public void SetupData()
    {
        _dataBase = new DataBaseService<GData>();
        _data = _dataBase.Load_Get();
    }

    public void Save()
    {
        _dataBase.Save(_data);
    }

    public class GData
    {
        public bool IsTutorial = false;
        public int LevelNumber = 1;
        public int Coins = 10;
        public int PremiumCoins = 10;
        public int TrophiesWinInGame = 0;
        public int AvailableLives = 5;
        public int Magnets = 0;
        public int Wand = 0;
        public int Hammer = 0;
        public int SlotPopper = 0;
        public int PlayerEarnedTrophies = 0;
        public int CurrentWinStreakLevel = 0;
        public List<PowerupType> OnBoardPowerUpType = new List<PowerupType>();
        public bool IsPuzzleManiaUnlocked = false;
        public bool IsLeaderBoardUnlocked = false;
        public int CurrentLevelEnemies = 0;
        public int TempCollectedEnemies = 0;
        public bool SoundOn = true;
        public bool MusicOn = true;
        public bool HapticsOn = true;
        public bool BackFromWin = false;
        public string PreviousHealthResetTime;
        public int CurrentDailyRewardWeek = 1;
        public int CurrentAchievementRewardWeek = 1;
    }
}