using System.ComponentModel;

public partial class SROptions
{
    [Category("Currencies")]
    public void AddCoin()
    {
        SignalBus.Publish(new AddCoinsSignal { Amount = 1 });
    }

    public void AddLeaderboardTrophy()
    {
        SignalBus.Publish(new DebugAddLeaderboardTrophySignal());
    }

    [Category("DailyRewards")]
    public void ForceMoveToNextDay()
    {
        SignalBus.Publish(new DebugForceMoveToNextDaySignal());
    }
}