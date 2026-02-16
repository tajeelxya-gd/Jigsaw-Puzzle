public class OnHammerEnableSignal : ISignal
{
    public bool IsEnable;
}
public class DestroyArmyThroughHammer : ISignal
{
    public ColorType colorType;
    public bool IsBoss;
    public Enemy enemy;
}