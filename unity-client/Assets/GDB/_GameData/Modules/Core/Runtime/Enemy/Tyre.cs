using DG.Tweening;
using UnityEngine;
public class Tyre : MediumStickMan
{
    [SerializeField] private Transform _rotator;
    private Tween _rotateTween;
    public override void Initialize(ColorType colorType, int health, bool OverideSpeed = false, float Speed = 1)
    {
        base.Initialize(colorType, health, OverideSpeed, Speed);
        CanAttack = false;
    }
    private void Rotate()
    {
        _rotateTween?.Kill();
        _rotateTween = _rotator.DORotate(new Vector3(10, 0, 0), 100f, RotateMode.LocalAxisAdd).SetSpeedBased().SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }
    private void Pause()
    {
        _rotateTween?.Pause();
    }
    public override void OnResume()
    {
        base.OnResume();
        if (_rotateTween == null || !_rotateTween.IsActive())
            Rotate();
        else
            _rotateTween.Play();

        CanInterect = false;
    }
    public override void OnReached(bool isFirst)
    {
        isReached = true;
        _navMeshAgent.enabled = false;
        Pause();
    }
    public override void Damage(float val)
    {
        base.Damage(val);
        ApplyHitEffect();
        // float scaleIncrement = 0.025f;
        // transform.localScale += Vector3.one * scaleIncrement;
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
            }, Color.gray6, 0.1f).SetEase(Ease.OutQuad))
            .Append(DOTween.To(() => _tempReflectColor, x =>
            {
                _tempReflectColor = x;
                ApplyReflect(_tempReflectColor);
            }, Color.black, 0.15f).SetEase(Ease.OutQuad))
            .Join(transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5, 0.5f).SetLink(gameObject))
            .Play();
    }
    protected override void KillMe()
    {
        transform.DOScale(Vector3.one * 0.1f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() => OnKillTweenComplete());
        onEnemyDieSignal.enemy = this;
        SignalBus.Publish(onEnemyDieSignal);
        _hitTween?.Kill();
        OnDie();
    }
}