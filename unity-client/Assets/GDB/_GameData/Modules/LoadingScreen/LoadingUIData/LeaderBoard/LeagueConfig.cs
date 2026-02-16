using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class LeagueConfig
{
    public LeagueRankType League;
    public int MinTrophies;
    public int MaxTrophies;

    public LeagueConfig(LeagueRankType league, int min, int max)
    {
        League = league;
        MinTrophies = min;
        MaxTrophies = max;
    }

    public bool ContainsTrophies(int trophies)
    {
        return trophies >= MinTrophies && trophies <= MaxTrophies;
    }
}