using UnityEngine;

public interface IEnemyProvider
{
    public Enemy ProvideEnemy(int _lastCheckedColumnIndex, ColorType colorType, Vector3 origin, float Range);
    public int GetColumnCount();
    public bool IsFirst(IMoveable moveable);
}