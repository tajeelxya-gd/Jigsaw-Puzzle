using DG.Tweening;
using UnityEngine;

public class EnemyCollectableUI : CollectableVisual
{
    [SerializeField] private RectTransform _icon;
    [SerializeField] private BulkPopUpEffect bulkPopUpEffect;

    [Header("Animation")]
    [SerializeField] private float _punchScale = 0.25f;
    [SerializeField] private float _punchDuration = 0.22f;
    [SerializeField] private Ease _punchEase = Ease.OutBack;
    [SerializeField] private ParticleSystem _starParticle;

    private Tween _iconPunchTween;

    public override void Initialize()
    {
        // if (!GlobalService.GameData.Data.IsPuzzleManiaUnlocked)
        // {
        //     gameObject.SetActive(false);
        //     return;
        // }
        GlobalService.GameData.Data.TempCollectedEnemies = 0;
        base.Initialize();
        _canCount = true;
        SignalBus.Subscribe<OnEnemyDieSignal>(OnEnemyDie);
    }
    private void OnEnemyDie(OnEnemyDieSignal signal)
    {
        bulkPopUpEffect.Generate(null, signal.Count);
        UpdateCount(signal.Count);
        PlayPopAnimation();
    }
    private bool _canCount = false;
    public override void UpdateCount(int count)
    {
        if (!_canCount)
        {
            _canCount = true;
            return;
        }
        GlobalService.GameData.Data.TempCollectedEnemies += count;
        base.UpdateCount(GlobalService.GameData.Data.TempCollectedEnemies);
        GlobalService.GameData.Data.CurrentLevelEnemies = GlobalService.GameData.Data.TempCollectedEnemies;
        GlobalService.GameData.Save();
    }
    private void OnDestroy()
    {
        _iconPunchTween?.Kill();
        SignalBus.Unsubscribe<OnEnemyDieSignal>(OnEnemyDie);
    }

    private void PlayPopAnimation()
    {
        Vector3 currentScale = _icon.localScale;
        _iconPunchTween?.Kill(false);
        float punchTarget = 1f + _punchScale;
        _iconPunchTween = DOTween.Sequence()
            .Append(
                _icon.DOScale(
                    Vector3.one * Mathf.Max(currentScale.x, punchTarget),
                    _punchDuration * 0.45f
                ).SetEase(Ease.OutBack)
            )
            .Append(
                _icon.DOScale(Vector3.one, _punchDuration * 0.55f)
                     .SetEase(Ease.OutQuad)
            )
            .SetUpdate(true)
            .OnComplete(PlayParticle);
    }
    private void PlayParticle()
    {
        if (_starParticle.isPlaying) return;
        _starParticle.Play();
    }
}