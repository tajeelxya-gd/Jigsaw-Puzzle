using System.ComponentModel;
using Client.Runtime;

public partial class SROptions
{
    [Category("Currencies")]
    public void Add100Coins()
    {
        GlobalService.GameData.Data.Coins += 100;
        GlobalService.GameData.Save();
    }

    [Category("Boosters")]
    public void Add5Magnets()
    {
        GlobalService.GameData.Data.Magnets += 5;
        GlobalService.GameData.Save();
    }

    [Category("Boosters")]
    public void Add5Eyes()
    {
        GlobalService.GameData.Data.Eye += 5;
        GlobalService.GameData.Save();
    }

    [Category("DailyRewards")]
    public void ForceMoveToNextDay()
    {
        SignalBus.Publish(new DebugForceMoveToNextDaySignal());
    }
}