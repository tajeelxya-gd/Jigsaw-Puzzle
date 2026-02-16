using DG.Tweening;
using UnityEngine;

public class Bullet : Projectile, IPoolable
{
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Renderer _renderer;
    private Tween _moveTween;

    public override void MoveToTarget()
    {
        if (!_lockedEnemy) return;

        Enemy enemy = _lockedEnemy;
        _lockedEnemy = null;
        transform.SetParent(null);
        _moveTween?.Kill();

        Transform target = enemy.transform;

        float distance = Vector3.Distance(transform.position, target.position);
        float duration = Mathf.Clamp(distance * 0.08f, 0.25f, 0.35f) * 1f;

        _moveTween = transform.DOMove(target.position + (Vector3.up * 0.5f), duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (target == null) return;

                Vector3 dir = (target.position - transform.position).normalized;
                if (dir.sqrMagnitude > 0.01f)
                    transform.forward = -dir;
            }).OnComplete(() =>
            {
                enemy.Damage(_damagerPerHit);
                PoolManager.GetPool<Projectile>().Release(this);
                HapticController.Vibrate(HapticType.Hit);
            });

    }

    public override void OnDespawned()
    {
        _trailRenderer.enabled = false;
        _trailRenderer.Clear();

        _moveTween?.Kill();
        _moveTween = null;

        _lockedEnemy = null;
    }

    public override void OnSpawned()
    {
        _trailRenderer.enabled = true;
    }
}
