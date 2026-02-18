
using UnityEngine;

public static class OnBoardingConfig
{
    private const string KEY = "OnBoardingConfig";
    public enum OnBoardingType { PuzzleMania = 1, TheGreatRace = 1, WeeklyRewards = 1, LeaderBoard = 1 }

    public static void SetOnBoardingDone(OnBoardingType onBoardingType)
    {
        PlayerPrefs.SetInt(KEY + onBoardingType, 1);
    }

    public static bool HasOnBoardingActivatedBefore(OnBoardingType onBoardingType)
    {
        return PlayerPrefs.GetInt(KEY + onBoardingType) == 1;
    }

    public static void ClearOnBoarding(OnBoardingType onBoardingType)
    {
        PlayerPrefs.DeleteKey(KEY + onBoardingType);
    }

    public static void SetUpOnBoardingDone(OnBoardingType onBoardingType)
    {
        PlayerPrefs.SetInt(KEY + onBoardingType, 1);
    }
}

