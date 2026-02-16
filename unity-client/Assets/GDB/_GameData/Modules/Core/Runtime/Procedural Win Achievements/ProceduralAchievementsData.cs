using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProceduralAchievementsData", menuName = "Achievements/ProceduralAchievementsData")]
public class ProceduralAchievementsData : ScriptableObject
{
    [ListDrawerSettings(Expanded = true)]
    [ValidateInput("ValidateOrder", "StartLevel of achievements must be in ascending order and non-overlapping.")]
    public List<ProceduralUnlockData> ProceduralAchievements = new List<ProceduralUnlockData>();

    private bool ValidateOrder(List<ProceduralUnlockData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var data = list[i];

            if (data.StartLevel > data.EndLevel)
                return false;

            if (i > 0)
            {
                var prev = list[i - 1];
                if (data.StartLevel <= prev.EndLevel)
                    return false;
            }
        }
        return true;
    }
    
    public class AchievementResult
    {
        public Sprite RewardSprite;
        public float FillAmount; // 0 to 1
    }

    public bool IsStartLevel(int completedLevel)
    {
        foreach (var data in ProceduralAchievements)
        {
            if (completedLevel >= data.StartLevel && completedLevel <= data.EndLevel)
            {
                return completedLevel == data.StartLevel;
            }
        }

        return false;
    }

    public AchievementResult GetAchievementProgress(int completedLevel)
    {
        foreach (var data in ProceduralAchievements)
        {
            if (completedLevel >= data.StartLevel && completedLevel <= data.EndLevel)
            {
                int range = data.EndLevel - data.StartLevel + 1;
                int progressInRange = completedLevel - data.StartLevel + 1;
                float fill = Mathf.Clamp01((float)progressInRange / range);

                return new AchievementResult
                {
                    RewardSprite = data.RewardSprite,
                    FillAmount = fill
                };
            }
        }
        
        // If completed level is beyond the last achievement, return last achievement fully filled
        if (ProceduralAchievements.Count > 0 && completedLevel > ProceduralAchievements[^1].EndLevel)
        {
            var last = ProceduralAchievements[^1];
            return new AchievementResult
            {
                RewardSprite = last.RewardSprite,
                FillAmount = 1f
            };
        }

        // If completed level is before the first achievement
        return new AchievementResult
        {
            RewardSprite = null,
            FillAmount = 0f
        };
    }
    
    
}

[System.Serializable]
public class ProceduralUnlockData
{
    [HorizontalGroup("Split", 0.7f)]
    public string Name;

    [HorizontalGroup("Split", 0.15f)]
    [MinValue(0)]
    public int StartLevel;

    [HorizontalGroup("Split", 0.15f)]
    [MinValue(0)]
    [ValidateInput("ValidateEndLevel", "EndLevel must be greater than or equal to StartLevel")]
    public int EndLevel;

    [PreviewField(100)]
    public Sprite RewardSprite;

    private bool ValidateEndLevel(int endLevel)
    {
        return endLevel >= StartLevel;
    }

    
}