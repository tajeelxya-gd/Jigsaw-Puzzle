using UnityEngine;
using System;
using System.IO;

public class RealTimeService : ITimeService
{
    private DateTime endTime;
    private bool isRunning;

    private bool pendingCompletion;      // expired while app closed
    private bool completionConsumed;     // reward already granted

    private string timerName;
    private string filePath;
    private Action onComplete;

    public RealTimeService(string name, Action callback)
    {
        timerName = name;
        filePath = Path.Combine(Application.persistentDataPath, $"{timerName}.json");
        onComplete = callback;

        LoadTimer();
    }

    public void StartTimer(float durationInSeconds)
    {
        endTime = DateTime.UtcNow.AddSeconds(durationInSeconds);
        isRunning = true;
        completionConsumed = false;   // reset
        pendingCompletion = false;

        SaveTimer();
    }

    public string GetFormattedTime()
    {
        float remainingTime = GetRemainingTime();
        int minutes = (int)(remainingTime / 60);
        int seconds = (int)(remainingTime % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    public string GetFormattedTimeHours()
    {
        throw new NotImplementedException();
    }

    public string GetFormattedTimeMinutes()
    {
        float remainingTime = GetRemainingTime();
        int minutes = (int)(remainingTime / 60);
        int seconds = (int)(remainingTime % 60);
        return $"{minutes}m {seconds:D2}s";
    }

    private float GetRemainingTime()
    {
        if (!isRunning) return 0;
        return Mathf.Max(0, (float)(endTime - DateTime.UtcNow).TotalSeconds);
    }

    public void ExtendTimer(int minutes)
    {
        if (minutes == 0) return;

        if (!isRunning || DateTime.UtcNow >= endTime)
        {
            endTime = DateTime.UtcNow.AddMinutes(minutes);
            isRunning = true;
            completionConsumed = false;
        }
        else
        {
            endTime = endTime.AddMinutes(minutes);
        }

        SaveTimer();
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public void Update()
    {
        // handle completion that happened while closed
        if (pendingCompletion && !completionConsumed)
        {
            pendingCompletion = false;
            completionConsumed = true;
            SaveTimer();
            onComplete?.Invoke();
            return;
        }

        if (!isRunning)
            return;

        if (DateTime.UtcNow >= endTime)
        {
            isRunning = false;

            if (!completionConsumed)
            {
                completionConsumed = true;
                SaveTimer();
                onComplete?.Invoke();
            }
        }
    }

    public void DeleteTimerFile()
    {
        throw new NotImplementedException();
    }

    private void LoadTimer()
    {
        if (!File.Exists(filePath)) return;

        try
        {
            TimerData data = JsonUtility.FromJson<TimerData>(File.ReadAllText(filePath));

            endTime = DateTime.Parse(data.endTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind);

            completionConsumed = data.completionConsumed;

            if (DateTime.UtcNow >= endTime)
            {
                isRunning = false;

                if (!completionConsumed)
                    pendingCompletion = true;
            }
            else
            {
                isRunning = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load timer: {e.Message}");
            isRunning = false;
        }
    }

    public void SaveTimer()
    {
        try
        {
            TimerData data = new TimerData
            {
                endTime = endTime.ToString("o"),
                completionConsumed = completionConsumed
            };

            File.WriteAllText(filePath, JsonUtility.ToJson(data));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save timer: {e.Message}");
        }
    }

    [Serializable]
    private class TimerData
    {
        public string endTime;
        public bool completionConsumed;
    }
}
