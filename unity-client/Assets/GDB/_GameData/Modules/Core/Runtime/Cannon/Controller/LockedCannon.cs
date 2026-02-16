using DG.Tweening;
using UnityEngine;

public class LockedCannon : Cannon
{
    [SerializeField] private Transform _lockObject;
    [SerializeField] private GameObject _runTrail;
    [SerializeField] private Transform KeyItem, EndPos;
    public override void Initialize(ColorType colorType, int projectileAmount, IEnemyProvider enemyProvider, ShooterType cannonType, bool isHidden = false)
    {
        base.Initialize(colorType, projectileAmount, enemyProvider, cannonType, isHidden);
        SignalBus.Subscribe<IKeyCollectionSignal>(KeyCollectionSignal);
        _haveKey = false;
    }

    private void OnDestroy()
    {
        _moveTween?.Kill();
        SignalBus.Unsubscribe<IKeyCollectionSignal>(KeyCollectionSignal);
    }

    public override void InitializeForShoot()
    {
        return;
    }

    public override void Remove()
    {
        _runTrail.SetActive(true);

        if (transform.position.x > 0)
        {
            transform.DORotate(new Vector3(0, -90, 0), 0.25f).OnComplete(() =>
            {
                transform.DOMoveX(5, 0.5f).OnComplete(() =>
                {
                    _key.transform.SetParent(null);
                    PoolManager.GetPool<Key>().Release(_key);
                    SignalBus.Publish(new OnShooterRemoveFromGridSignal { shootable = new System.Collections.Generic.List<IShootable> { this } });
                    Destroy(gameObject);
                });
            });
        }
        else if (transform.position.x <= 0)
        {
            transform.DORotate(new Vector3(0, 90, 0), 0.25f).OnComplete(() =>
            {
                transform.DOMoveX(-5, 0.5f).OnComplete(() =>
                {
                    _key.transform.SetParent(null);
                    PoolManager.GetPool<Key>().Release(_key);
                    SignalBus.Publish(new OnShooterRemoveFromGridSignal { shootable = new System.Collections.Generic.List<IShootable> { this } });
                    Destroy(gameObject);
                });
            });
        }
    }

    public override void Shot()
    {
        return;
    }

    private bool _haveKey = false;
    private Key _key;
    private void KeyCollectionSignal(IKeyCollectionSignal signal)
    {
        if (_haveKey) return;
        if (!IsFirstInRow) return;
        if (signal.key.IsCollected) return;
        if (!_enemyProvider.IsFirst(signal.key)) return;
        _haveKey = true;
        _key = signal.key;
        _key.Collect();
        AudioController.PlaySFX(AudioType.KeyCollect, 0.5f);
        DOVirtual.DelayedCall(0.2f, () => AnimateKey());
    }

    private float mul = 0.3f;
    private Tween _moveTween;
    private void AnimateKey()
    {
        KeyItem.position = _key.transform.position;
        KeyItem.gameObject.SetActive(true);

        KeyItem.DOPunchScale(-(Vector3.one * 0.5f), 2 * mul, 3)
               .SetEase(Ease.OutCirc);

        float duration = 2 * mul;
        Vector3 startPos = KeyItem.position;

        float jumpHeight = 4f;

        _moveTween = DOTween.Sequence()
            .Append(DOTween.To(() => 0f, t =>
            {
                Vector3 targetPos = EndPos.position;
                Vector3 horizontal = Vector3.Lerp(startPos, targetPos, t);

                float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;

                KeyItem.position = new Vector3(
                    horizontal.x,
                    horizontal.y + yOffset,
                    horizontal.z
                );

            }, 1f, duration).SetEase(Ease.OutCirc))
            .OnComplete(() =>
            {
                KeyItem.DORotate(new Vector3(0, 360, 180), 0.7f * mul);
                _lockObject.DOLocalMove(new Vector3(0, 0, -0.8f), 0.7f * mul)
                           .SetDelay(0.05f)
                           .OnComplete(Remove);
            });
    }
}