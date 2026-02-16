using System.Collections.Generic;

public static class LeagueConfigManager
{
    public static List<LeagueConfig> LeagueConfigs = new List<LeagueConfig>
    {
        new LeagueConfig(LeagueRankType.Bronze, 1, 170),
        new LeagueConfig(LeagueRankType.Silver, 171, 250),
        new LeagueConfig(LeagueRankType.Gold, 251, 350),
        new LeagueConfig(LeagueRankType.Diamond, 351, 450)
    };

    public static LeagueConfig GetConfig(LeagueRankType league)
    {
        return LeagueConfigs.Find(c => c.League == league);
    }
    
    public static LeagueRankType GetLeagueByTrophies(int trophies)
    {
        foreach (var config in LeagueConfigs)
        {
            if (trophies >= config.MinTrophies && trophies <= config.MaxTrophies)
                return config.League;
        }
        return LeagueConfigs[^1].League;
    }
}