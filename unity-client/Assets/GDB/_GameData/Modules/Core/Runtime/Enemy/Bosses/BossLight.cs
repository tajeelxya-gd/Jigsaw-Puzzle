using UnityEngine;

public class BossLight : BossEnemy
{
    public override void OnKillTweenComplete()
    {
        PoolManager.GetPool<BossLight>().Release(this);
    }
}