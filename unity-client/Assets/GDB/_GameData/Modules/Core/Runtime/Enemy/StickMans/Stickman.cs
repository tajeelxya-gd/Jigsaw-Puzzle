using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stickman : Enemy, IAttackable
{
    [SerializeField][ShowIf("@_useShaderAnim == true")] private int[] _attackAnimNumbers;
    public override void Initialize(ColorType colorType, int health, bool OverideSpeed = false, float Speed = 1)
    {
        base.Initialize(colorType, health);
        ColorType = colorType;
        _canLookedMultipleTimes = false;
        CanAttack = true;
    }

    public override void OnBulletLock()
    {
        isLocked = true;
        // _canLookedMultipleTimes = false;
    }

    public override void Damage(float val)
    {
        KillMe();
    }
    private protected OnEnemyDieSignal onEnemyDieSignal = new OnEnemyDieSignal();
    protected virtual void KillMe()
    {
        isLocked = true;
        _canLookedMultipleTimes = false;
        transform.DOScale(Vector3.one * 0.1f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() => OnKillTweenComplete());
        onEnemyDieSignal.enemy = this;
        SignalBus.Publish(onEnemyDieSignal);
        OnDie();
    }
    public virtual void OnKillTweenComplete()
    {
        PoolManager.GetPool<Stickman>().Release(this);
    }
    protected virtual void OnDie()
    {
        GlobalService.ParticleService.PlayParticle(ParticleType.Hit, transform.position + (Vector3.up * 0.6f), true);
        GlobalService.ParticleService.PlayParticle(ParticleType.Shatter, transform.position + (Vector3.up * 0.6f), true, true, ColorProvider.GetColor(ColorType));
    }
    public override void OnSpawned()
    {
        transform.localScale = Vector3.one;
    }
    public override void OnDespawned()
    {
        _isDestined = false;
        // isLocked = false;
        _speedTween?.Kill();
    }

    private float _attackTimer = 3f;
    private float AttackDelay = 2f;
    private protected int _maxFightTypeCount = 3;

    public bool CanAttack { get; set; }

    private bool _isStunned => TutorialManager.IsTutorialActivated;

    public virtual void Attack()
    {
        if (_isStunned) return;
        if (!CanAttack) return;
        _attackTimer += Time.deltaTime;
        if (_attackTimer < AttackDelay) return;
        _attackTimer = 0f;
        DoAttack();
    }
    private protected WallDamageSignal _wallDamageSignal = new WallDamageSignal();
    private protected void DoAttack()
    {
        if (_useShaderAnim)
        {
            int randomFightMode = Random.Range(0, _attackAnimNumbers.Length);
            if (_attackAnimNumbers[randomFightMode] == 4)
                SetShaderAnimSpeed(1.5f);
            else
                SetShaderAnimSpeed(0.75f);
            PlayShaderAnim(_attackAnimNumbers[randomFightMode]);
        }
        else
        {
            if (_animator)
                _animator.SetTrigger("Fight");
            int randomFightMode = Random.Range(0, _maxFightTypeCount);
            if (_animator)
                _animator.SetInteger("FightStyle", randomFightMode);
        }

        _wallDamageSignal.DamageAmount = _enemyConfig.Damage;
        _wallDamageSignal.XPos = transform.position.x;
        SignalBus.Publish(_wallDamageSignal);
        AttackDelay = Random.Range(1.5f, 3);
    }
}