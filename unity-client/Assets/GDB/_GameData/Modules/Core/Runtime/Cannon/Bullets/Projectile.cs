using UnityEngine;

public abstract class Projectile : MonoBehaviour, IPoolable
{
    private protected Enemy _lockedEnemy;

    private protected float _damagerPerHit;

    public virtual void Initialize(float HitDamage)
    {
        _damagerPerHit = HitDamage;
        _lockedEnemy = null;
    }
    public virtual void LockTarget(Enemy enemy)
    {
        _lockedEnemy = enemy;
        MoveToTarget();
    }
    // private void OnTriggerEnter(Collider other)
    // {
    //     // if (other.TryGetComponent(out IDamageable enemy))
    //     // {
    //     //     if (enemy is Enemy enemy1)
    //     //     {
    //     //         if (_lockedEnemy == enemy1)
    //     //         {
    //     //             enemy.Damage(_damagerPerHit);
    //     //             PoolManager.GetPool<Projectile>().Release(this);
    //     //             HapticController.Vibrate(Lofelt.NiceVibrations.HapticPatterns.PresetType.LightImpact);
    //     //         }
    //     //     }
    //     // }
    // }

    public abstract void MoveToTarget();
    public abstract void OnDespawned();
    public abstract void OnSpawned();
}