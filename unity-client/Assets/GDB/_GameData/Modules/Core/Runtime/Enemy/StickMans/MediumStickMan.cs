using DG.Tweening;
using UnityEngine;

public class MediumStickMan : Stickman
{
    private protected int _remainingHits;
    public override void Initialize(ColorType colorType, int health, bool OverideSpeed = false, float Speed = 1)
    {
        base.Initialize(colorType, health);
        if (_enemyType == EnemyType.BulkyStickMan) Health = 1;
        _remainingHits = Health;
        _canLookedMultipleTimes = Health > 1;
        _maxFightTypeCount = 1;
    }
    public override void Damage(float val)
    {
        Health -= (int)val;

        if (Health <= 0)
        {
            KillMe();
            return;
        }

        _canLookedMultipleTimes = _remainingHits > 0;
    }
    protected override void KillMe()
    {
        transform.DOScale(Vector3.one * 0.1f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() => OnKillTweenComplete());
        // SignalBus.Publish(new OnEnemyDieSignal { enemy = this });
        SignalBus.Publish(new OnSpawnEnemyAtSignal { colorType = ColorType, enemyType = EnemyType.Simple, moveable = this });
        OnDie();
    }
    public override void OnBulletLock()
    {
        _remainingHits--;
        _canLookedMultipleTimes = _remainingHits > 0;
        base.OnBulletLock();
    }
    public override void OnKillTweenComplete()
    {
        PoolManager.GetPool<MediumStickMan>().Release(this);
    }

    public class OnSpawnEnemyAtSignal : ISignal
    {
        public ColorType colorType;
        public EnemyType enemyType;
        public IMoveable moveable;
    }
}