using DG.Tweening;
using UnityEngine;

public class Key : Stickman, IAutoCollectable
{
    public override void Initialize(ColorType colorType, int health, bool OverideSpeed = false, float Speed = 1)
    {
        if (_navMeshAgent.obstacleAvoidanceType == UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance)
            _navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.MedQualityObstacleAvoidance;

        if (OverideSpeed)
            _navMeshAgent.speed = Speed;
        else
            SpeedCurve();

        Health = health;
        ColorType = colorType;
        _canLookedMultipleTimes = health > 1;
        CanAttack = false;
        CanMove = true;
        SignalBus.Subscribe<OnShooterRemoveFromGridSignal>(OnShooterRemove);
    }

    private IKeyCollectionSignal _keyCollectionSignal = new IKeyCollectionSignal();
    public void AutoCollect()
    {
        _keyCollectionSignal.key = this;
        SignalBus.Publish(new IKeyCollectionSignal { key = this });
    }
    public bool IsCollected { get; private set; }
    public void Collect()
    {
        IsCollected = true;
        onEnemyDieSignal.enemy = this;
        SignalBus.Publish(onEnemyDieSignal);
        OnPause();
        _navMeshAgent.enabled = false;
        KillMe();
    }
    protected override void KillMe()
    {
        transform.DOScale(Vector3.one * 0.1f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() => OnKillTweenComplete());
        // SignalBus.Publish(new OnEnemyDieSignal { enemy = this });
        OnDie();
    }
    public override void OnKillTweenComplete()
    {
        PoolManager.GetPool<Key>().Release(this);
    }
    protected override void OnDie()
    {
        GlobalService.ParticleService.PlayParticle(ParticleType.Hit, transform.position + (Vector3.up * 0.6f), true);
        GlobalService.ParticleService.PlayParticle(ParticleType.StarEffect, transform.position + (Vector3.up * 0.6f), true);
    }
    private void OnShooterRemove(ISignal signal)
    {
        AutoCollect();
    }
    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnShooterRemoveFromGridSignal>(OnShooterRemove);
    }
}
public interface IAutoCollectable : ICollectable { public void AutoCollect(); }
public class IKeyCollectionSignal : ISignal { public Key key; }