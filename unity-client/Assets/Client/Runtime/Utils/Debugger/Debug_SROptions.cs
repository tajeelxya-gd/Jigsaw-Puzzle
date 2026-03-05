using System.ComponentModel;

public partial class SROptions
{
    [Category("DailyRewards")]
    public void ForceMoveToNextDay()
    {
        SignalBus.Publish(new DebugForceMoveToNextDaySignal());
    }
}