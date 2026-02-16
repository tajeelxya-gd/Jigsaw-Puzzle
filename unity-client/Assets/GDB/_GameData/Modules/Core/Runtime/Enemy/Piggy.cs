using DG.Tweening;
using UnityEngine;

public class Piggy : MediumStickMan, ICollectable
{
    [SerializeField] private Transform _CoinObj;

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
        _remainingHits = health;
        _canLookedMultipleTimes = health > 1;
        CanAttack = false;
        CanMove = true;
        _CoinObj.localScale = Vector3.one;
    }
    public override void Damage(float val)
    {
        base.Damage(val);
        PlayCoinAnimation();
        float scaleIncrement = 0.025f;
        transform.localScale += Vector3.one * scaleIncrement;
    }
    protected override void KillMe()
    {
        transform.DOScale(Vector3.one * 0.1f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() => OnKillTweenComplete());
        onEnemyDieSignal.enemy = this;
        onEnemyDieSignal.IsSpecial = true;
        SignalBus.Publish(onEnemyDieSignal);
        SignalBus.Publish(new OnPiggyKilledSignal());
        OnDie();
    }
    public override void OnKillTweenComplete()
    {
        _coinTween?.Kill();
        PoolManager.GetPool<Piggy>().Release(this);
    }
    protected override void OnDie()
    {
        GlobalService.ParticleService.PlayParticle(ParticleType.Hit, transform.position + (Vector3.up * 0.6f), true);
        GlobalService.ParticleService.PlayParticle(ParticleType.CoinBurst, transform.position + (Vector3.up * 0.6f), true);
        AudioController.PlaySFX(AudioType.Piggy, 1, 0.7f);
    }

    private Tween _coinTween;
    private void PlayCoinAnimation()
    {
        if (_coinTween != null && _coinTween.IsActive() && _coinTween.IsPlaying())
            return;

        Transform originalParent = _CoinObj.parent;

        _CoinObj.SetParent(null, true);

        _CoinObj.position = transform.position + new Vector3(0, 0.7f, 0);
        _CoinObj.rotation = Quaternion.identity;
        _CoinObj.localScale = Vector3.one;

        Tween rotateTween = _CoinObj.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);

        _coinTween = DOTween.Sequence()
            .Append(_CoinObj.DOMoveY(_CoinObj.position.y + 0.8f, 0.3f).SetEase(Ease.OutCubic))
            .Join(_CoinObj.DOScale(3f, 0.3f).SetEase(Ease.OutCubic))
            .Append(_CoinObj.DOMoveY(_CoinObj.position.y, 0.3f).SetEase(Ease.InCubic))
            .Join(_CoinObj.DOScale(1f, 0.3f).SetEase(Ease.InCubic))
            .OnKill(() =>
            {
                rotateTween.Kill();
                _CoinObj.SetParent(originalParent, true);
            })
            .OnComplete(() =>
            {
                rotateTween.Kill();
                _CoinObj.SetParent(originalParent, true);
            })
            .Play();

        rotateTween.Play();
    }
}