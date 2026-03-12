using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Wall : MonoBehaviour, IDamageable
{
    [SerializeField] private int _health;
    [SerializeField] private WallView _wallView;

    [SerializeField] private Material _wallMaterial;

    [SerializeField] private Transform[] _wallsPieces;

    private float _tempHealth;

    public void Initialize()
    {
        // _health = RemoteConfigManager.Configuration.WallHealth;
        _tempHealth = _health;
        _isWallFall = false;
        _currentPieceIndex = -1;
        _wallView.Initialize(_health);
        InitializeWallPieces();
        SignalBus.Subscribe<WallDamageSignal>(WallDamageSignal);
        SignalBus.Subscribe<OnWallRevivalSignal>(ReviveHealth);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<WallDamageSignal>(WallDamageSignal);
        SignalBus.Unsubscribe<OnWallRevivalSignal>(ReviveHealth);
    }

    private bool _isWallFall = false;
    public void Damage(float val)
    {
        if (_isWallFall) return;
        _tempHealth -= val;
        _wallView.UpdateBar(_tempHealth);
        AnimateWallHit();
        UpdateWallVisual();
        if (_tempHealth <= 0)
        {
            _isWallFall = true;
            Debug.LogError("WALL BREAK");
            // SignalBus.Publish(new OnlevelFailSignal { levelFailType = LevelFailType.WallBreak });
        }
    }

    private void ReviveHealth(OnWallRevivalSignal signal)
    {
        _health = 3000;
        _tempHealth = _health;
        _isWallFall = false;
        _currentPieceIndex = -1;
        InitializeWallPieces();
        UpdateWallVisual();
    }
    private Tween _hitTween;
    private void AnimateWallHit()
    {
        if (_hitTween != null && _hitTween.IsActive()) return;

        _hitTween = transform
                   .DOShakePosition(0.2f, new Vector3(0, 0.1f, 0), vibrato: 10, randomness: 0, snapping: false, fadeOut: true)
                   .SetEase(Ease.OutQuad);
    }
    private const float MinSfxInterval = 1f;
    private float _lastSfxTime;
    private void WallDamageSignal(WallDamageSignal signal)
    {
        Damage(signal.DamageAmount);

        if (Time.unscaledTime - _lastSfxTime > MinSfxInterval)
        {
            _lastSfxTime = Time.unscaledTime;
            AudioController.PlaySFX(AudioType.WallHit, 0.2f);
        }

        if (Random.value > 0.6f)
            GlobalService.ParticleService.PlayParticle(ParticleType.Hit, new Vector3(signal.XPos, 0.3f, -0.85f), true);
        else
            GlobalService.ParticleService.PlayParticle(ParticleType.Shatter, new Vector3(signal.XPos, 0.3f, -0.85f), true, true, _wallMaterial.color);
    }

    private int _currentPieceIndex = -1;
    private void InitializeWallPieces()
    {
        for (int i = 0; i < _wallsPieces.Length; i++)
            _wallsPieces[i].gameObject.SetActive(false);

        _wallsPieces[0].gameObject.SetActive(true);
        _currentPieceIndex = 0;
    }
    private void UpdateWallVisual()
    {
        float healthPercent = _tempHealth / _health;

        int newIndex = Mathf.FloorToInt(
            (1f - healthPercent) * _wallsPieces.Length
        );

        newIndex = Mathf.Clamp(newIndex, 0, _wallsPieces.Length - 1);

        if (newIndex == _currentPieceIndex)
            return;

        SwitchWallPiece(newIndex);
    }
    private void SwitchWallPiece(int index)
    {
        for (int i = 0; i < _wallsPieces.Length; i++)
            _wallsPieces[i].gameObject.SetActive(i == index);

        _currentPieceIndex = index;
    }
}

public class OnWallRevivalSignal : ISignal
{ }