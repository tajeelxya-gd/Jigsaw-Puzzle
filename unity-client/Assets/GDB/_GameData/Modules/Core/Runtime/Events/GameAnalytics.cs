using System.Collections.Generic;
using Monetization.Runtime.Analytics;
using UnityEngine;

public static class GameAnalytics
{
    public static int MagnetUsed = 0;
    public static int ShufflerUsed = 0;
    public static int HammerUsed = 0;
    public static int SlotPopperUsed = 0;
    public static void PublishAnalytic(AnalyticEventType name, params string[] param)
    {
        AnalyticsManager.SendAppMetricaEvent(name.ToString(), param);
    }
    public static void ResetBoosterData()
    {
        MagnetUsed = 0;
        ShufflerUsed = 0;
        HammerUsed = 0;
        SlotPopperUsed = 0;
    }
}
public enum AnalyticEventType
{
    GameData,
    Extra,
    DailyRewards,
    PuzzleMania,
    AchievementEvent,
    RaceEvent
}
public enum GameEventType
{
    Progression,
    Events
}