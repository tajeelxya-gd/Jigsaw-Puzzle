using DG.Tweening;
using TMPro;
using UnityEngine;

public class BossEnemy : MediumStickMan
{
    [SerializeField] private TextMeshPro _healthAmount;

    public override void Initialize(ColorType colorType, int health, bool OverideSpeed = false, float Speed = 1)
    {
        base.Initialize(colorType, health);
        UpdateHealthText();
        _maxFightTypeCount = 1;
    }
    public override void OnBulletLock()
    {
        base.OnBulletLock();
        _healthAmount.gameObject.SetActive(true);
    }
    public override void Damage(float val)
    {
        base.Damage(val);
        UpdateHealthText();
        ApplyHitEffect();
    }
    protected override void KillMe()
    {
        transform.DOScale(Vector3.one * 0.1f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() => OnKillTweenComplete());
        SignalBus.Publish(new OnEnemyDieSignal { enemy = this });
        OnDie();
    }
    private Tween _hitTween;
    private Color _tempReflectColor = Color.black;

    private void ApplyHitEffect()
    {
        if (_hitTween != null && _hitTween.IsActive() && _hitTween.IsPlaying())
            return;

        _hitTween = DOTween.Sequence()
            .Append(DOTween.To(() => _tempReflectColor, x =>
            {
                _tempReflectColor = x;
                ApplyReflect(_tempReflectColor);
            }, Color.grey, 0.1f).SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => _tempReflectColor, x =>
            {
                _tempReflectColor = x;
                ApplyReflect(_tempReflectColor);
            }, Color.black, 0.15f).SetEase(Ease.OutQuad))
            .Join(transform.DOPunchScale(Vector3.one * -0.1f, 0.2f, 5, 0.5f).SetLink(gameObject))
            .Play();
    }
    private void UpdateHealthText()
    {
        _healthAmount.text = Health.ToString();
    }
    public override void OnKillTweenComplete()
    {
        _hitTween?.Kill();
        PoolManager.GetPool<BossEnemy>().Release(this);
    }
}