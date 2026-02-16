public class OnlevelFailSignal : ISignal
{
    public LevelFailType levelFailType;
}
public enum LevelFailType
{
    none,
    OutOFSpace,
    WallBreak
}