public interface ITimeService
{
    public void StartTimer(float durationInSeconds);
    public string GetFormattedTime();
    public string GetFormattedTimeHours();
    public string GetFormattedTimeMinutes();
    public void ExtendTimer(int minutes);
    public bool IsRunning();
    public void SaveTimer();
    public void Update();
    public void DeleteTimerFile();
}
