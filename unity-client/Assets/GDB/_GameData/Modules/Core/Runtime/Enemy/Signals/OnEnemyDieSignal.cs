public class OnEnemyDieSignal : ISignal
{
    public Enemy enemy;
    public bool IsSpecial = false;
    public int Count = 1;
}