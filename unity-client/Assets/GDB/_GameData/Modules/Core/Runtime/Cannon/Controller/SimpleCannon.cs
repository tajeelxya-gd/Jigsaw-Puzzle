using System;
using System.Collections.Generic;
using DG.Tweening;
using Lofelt.NiceVibrations;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class SimpleCannon : Cannon, IMoveable, IRejection, ILink<Cannon>, IColorLerpable
{
    public bool CanMove { get; set; }

    [InfoBox("Blend Shape Anim")]
    [SerializeField] private SkinnedMeshRenderer _meshFilter;
    [SerializeField][Range(0.1f, 3)] private float _animSpeed = 1;

    [InfoBox("Visual")]
    [SerializeField] private TextMeshPro _amountVisualTxt;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Transform _turret;
    [SerializeField] private int _matColorIndex = 1;
    [SerializeField] private GameObject _runTrail;


    private MaterialPropertyBlock _propertyBlock;
    private Tween _breathTween;
    private Texture _gradientTexture;

    public override void Initialize(ColorType colorType, int projectileAmount, IEnemyProvider enemyProvider, ShooterType cannonType, bool isHidden = false)
    {
        base.Initialize(colorType, projectileAmount, enemyProvider, cannonType, isHidden);

        _propertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_propertyBlock);

        _gradientTexture = _renderer.material.mainTexture;

        SignalBus.Subscribe<OnMagnetEnableSignal>(OnMagnetPowerUp);

        if (IsHidden)
        {
            OnHidden();
            return;
        }

        UpdateTxt();
        SetColor(ColorProvider.GetColor(_colorType));
        ChangeTextAlpha(0.3f);

        CanMove = true;

        //transform.DOScale(0.9f, 1).SetEase(Ease.OutBack);
    }

    private bool _canShoot = false;
    public override void InitializeForShoot()
    {
        _canShoot = true;
        CanInterect = false;
        ChangeTextAlpha(1);
        HandleOutLine(false);
    }
    public override void SetFirstInRow()
    {
        base.SetFirstInRow();
        if (IsHidden) OnHiddenUnlock();
        ChangeTextAlpha(1);
        transform.localScale = Vector3.one;
        BreathAnimation();
        HandleOutLine(true);
    }
    public override void SetNotFirstInRow()
    {
        base.SetNotFirstInRow();
        if (IsHidden) OnHidden();
        else
            ChangeTextAlpha(0.3f);
        transform.DOScale(0.9f, 1).SetEase(Ease.OutBack);
        StopBreathing();
        HandleOutLine(false);
    }

    private void BreathAnimation()
    {
        _breathTween?.Kill();
        _breathTween = transform.DOLocalMoveY(0.2f, 1f).SetLoops(-1, LoopType.Yoyo);
    }
    private void StopBreathing()
    {
        if (_breathTween != null)
        {
            _breathTween?.Kill();
            transform.DOLocalMoveY(0, 0.1f);
        }
    }
    [Button]
    private void HandleOutLine(bool enable)
    {
        // if (!enable)
        // {
        //     // _propertyBlock.SetFloat("_UseOutline", 0);
        //     _propertyBlock.SetFloat("_OutlineWidth", 0);
        //     UnityEngine.Debug.LogError(_propertyBlock.GetFloat("_OutlineWidth"));
        // }
        // else
        // {
        //     // _propertyBlock.SetFloat("_UseOutline", 1);
        //     _propertyBlock.SetFloat("_OutlineWidth", 3.95f);
        //     UnityEngine.Debug.LogError(_propertyBlock.GetFloat("_OutlineWidth"));
        //     _propertyBlock.SetColor("_OutlineColor", ColorProvider.GetColor(_colorType) * 0.7f);
        // }
        // _renderer.SetPropertyBlock(_propertyBlock);
    }

    private float _fireCooldown;
    private Tween _turretTween;
    private int _lastCheckedColumnIndex = 0;
    private int _shotsSinceRandom = 0;
    private const int RANDOM_RESET_THRESHOLD = 10;
    public bool Debug;

    public override void Shot()
    {
        if (!_canShoot) return;
        if (_projectileAmount <= 0) return;
        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
            return;
        }
        if (_enemyProvider == null) return;

        Enemy enemy = _enemyProvider.ProvideEnemy(_lastCheckedColumnIndex, _colorType, transform.position, _cannonConfig.Range);

        if (enemy != null)
        {
            _shotAvailable = true;

            _turretTween?.Kill();
            Vector3 dir = (enemy.transform.position - _turret.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(-dir);

            _turretTween = _turret
         .DORotateQuaternion(targetRot, 0.1f)
         .SetEase(Ease.OutQuad).SetUpdate(false);

            PlayShootAnim();
            FireProjectile(enemy);
            _fireCooldown = _cannonConfig.RateOfFire;

            if (enemy is ICollectable) return;

            _projectileAmount--;
            UpdateTxt();

            if (enemy.Health <= 1)
            {
                _lastCheckedColumnIndex = (_lastCheckedColumnIndex + 1) % _enemyProvider.GetColumnCount();
                _shotsSinceRandom++;
                if (_shotsSinceRandom >= RANDOM_RESET_THRESHOLD)
                {
                    _lastCheckedColumnIndex = UnityEngine.Random.Range(0, _enemyProvider.GetColumnCount());
                    _shotsSinceRandom = 0;
                }
            }

            if (_projectileAmount <= 0)
            {
                _canShoot = false;
                _shotAvailable = false;
                if (IsLinked)
                {
                    if (LinkedObject.TotalProjectileRemaining <= 0)
                    {
                        SignalBus.Publish(new OnShooterEmptySignal { shootable = this });
                        if (LinkedObject && LinkedObject.gameObject)
                        {
                            SignalBus.Publish(new OnShooterEmptySignal { shootable = LinkedObject });
                            LinkedObject.Remove();
                        }
                        Remove();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }

                SignalBus.Publish(new OnShooterEmptySignal { shootable = this });
                Remove();
            }
        }
        else
            _shotAvailable = false;
    }
    public override void Remove()
    {
        _canShoot = false;
        _turretTween?.Kill();
        _turret.eulerAngles = new Vector3(_turret.eulerAngles.x, 0, 0);
        _runTrail.SetActive(true);
        _amountVisualTxt.gameObject.SetActive(false);

        if (_tempLink)
            Destroy(_tempLink.gameObject);

        if (transform.position.x > 0)
        {
            transform.DORotate(new Vector3(0, -90, 0), 0.25f).OnComplete(() =>
            {
                transform.DOMoveX(5, 0.5f).SetUpdate(false).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
            });
        }
        else if (transform.position.x <= 0)
        {
            transform.DORotate(new Vector3(0, 90, 0), 0.25f).SetUpdate(false).OnComplete(() =>
            {
                transform.DOMoveX(-5, 0.5f).SetUpdate(false).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
            });
        }
    }
    public virtual void FireProjectile(Enemy enemy)
    {
        Projectile _projectile = PoolManager.GetPool<Projectile>().Get();
        _projectile.Initialize(_cannonConfig.DamagePerHit);
        _projectile.transform.SetParent(_bulletPoint);
        _projectile.transform.localPosition = Vector3.zero;
        _projectile.transform.localEulerAngles = Vector3.zero;
        _projectile.LockTarget(enemy);
        SignalBus.Publish(_onEvaluateSpacesSignal);
        AudioController.PlaySFX(AudioType.Hit, 0.2f, 1.2f);
    }

    private Tween _cannonTween;
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private float _tweenSpeed = 0.1f;
    private void PlayShootAnim()
    {
        if (!_skinnedMeshRenderer)
            _skinnedMeshRenderer = _renderer.GetComponent<SkinnedMeshRenderer>();

        if (_cannonTween != null && _cannonTween.IsActive() && _cannonTween.IsPlaying())
            return;

        float duration = 1f * _tweenSpeed;

        _cannonTween = DOTween.Sequence()
            .Append(DOVirtual.Float(0, 100, duration, x => _skinnedMeshRenderer.SetBlendShapeWeight(0, x)))
            .Append(DOVirtual.Float(100, 0, duration, x => _skinnedMeshRenderer.SetBlendShapeWeight(0, x)))
            .Append(DOVirtual.Float(0, 100, duration, x => _skinnedMeshRenderer.SetBlendShapeWeight(1, x)))
            .Append(DOVirtual.Float(100, 0, duration, x => _skinnedMeshRenderer.SetBlendShapeWeight(1, x)))
            .SetEase(Ease.Linear);
        GlobalService.ParticleService.PlayParticle(ParticleType.CannonShot, _bulletPoint, true);
    }
    private OnEvaluateSpacesSignal _onEvaluateSpacesSignal = new OnEvaluateSpacesSignal();
    public void Move(Vector3 target)
    {
        StopBreathing();
        transform.DOJump(target, 2, 1, 0.5f).OnComplete(() =>
        {
            _canShoot = true;
            DOVirtual.DelayedCall(1f, () => SignalBus.Publish(_onEvaluateSpacesSignal));
        });
    }

    private Tween _shakeTween;
    private Quaternion _startRotation = Quaternion.identity;
    public void Reject()
    {
        if (_canShoot) return;

        if (_startRotation != quaternion.identity)
            _startRotation = transform.localRotation;

        _shakeTween?.Kill();
        transform.localRotation = _startRotation;
        _shakeTween = transform
            .DOShakeRotation(0.2f, 15f, 15, 90f, true)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject).OnKill(() =>
        {
            transform.localRotation = _startRotation;
        });

        HapticController.VibrateForcefully(HapticType.Wrong);
    }
    public override void UpdateProjectile(int Number)
    {
        base.UpdateProjectile(Number);
        UpdateTxt();
    }
    private void UpdateTxt()
    {
        _amountVisualTxt.text = _projectileAmount.ToString();
    }
    private void SetColor(Color color)
    {
        _propertyBlock.SetColor("_BaseColor", color);
        _renderer.SetPropertyBlock(_propertyBlock, _matColorIndex);
        // _renderer.SetColor(color, "_BaseColor", _matColorIndex);
    }
    private void ChangeTextAlpha(float a)
    {
        _amountVisualTxt.color = new Color(_amountVisualTxt.color.r, _amountVisualTxt.color.g, _amountVisualTxt.color.b, a);
    }
    private protected void ApplyReflect(Color color)
    {
        _propertyBlock.SetColor("_EmissionColor", color);
        _renderer.SetPropertyBlock(_propertyBlock, _matColorIndex);
    }

    public bool IsLinked { get; set; }
    public bool AllFree { get { return IsFree(); } }
    public Cannon LinkedObject { get { return _linked; } }
    private Link _tempLink;
    private Cannon _linked;
    private Color _hiddenColor = new Color(0.3568628f, 0.3529412f, 0.7490196f);
    public Action OnOtherLinkVisible;
    public void Link(Cannon item, bool linkAgain = false)
    {
        _linked = item;

        if (linkAgain)
        {
            ILink<Cannon> otherLink = item as ILink<Cannon>;
            if (otherLink != null)
            {
                otherLink.IsLinked = true;
                otherLink.Link(this, false);
            }
        }

        if (IsLinked) return;
        IsLinked = true;

        Transform thisTransform = transform;
        Transform otherTransform = item.transform;
        _tempLink = Instantiate(Resources.Load<Link>("Link"));
        _tempLink.Initialize(thisTransform, otherTransform);

        _tempLink.ApplyColorToFirstRendrer(!IsHidden ? ColorProvider.GetColor(_colorType) : _hiddenColor);
        _tempLink.ApplyColorToSecondRendrer(!item.IsHidden ? ColorProvider.GetColor(item._colorType) : _hiddenColor);

        LinkedObject.GetComponent<SimpleCannon>().OnOtherLinkVisible = UpdateLinkColor;

        if (IsFirstInRow)
        {
            _tempLink.HandleOutline(true);
        }
    }

    private bool IsFree()
    {
        if (IsFirstInRow) return true;
        if (IsLinked)
        {
            if (LinkedObject != null &&
                LinkedObject.ColumnId == ColumnId &&
                Mathf.Abs(LinkedObject.transform.position.z - transform.position.z) <= 1.6f)
            {
                return true;
            }
        }
        return false;
    }

    private void OnHidden()
    {
        SetColor(Color.white);

        _propertyBlock.SetTexture("_BaseMap", ColorProvider.GetQuestionMarkTexture(_cannonType));
        _renderer.SetPropertyBlock(_propertyBlock, _matColorIndex);

        _amountVisualTxt.text = "?";
        ChangeTextAlpha(1f);
    }
    private void OnHiddenUnlock()
    {
        _propertyBlock.SetTexture("_BaseMap", _gradientTexture);
        _renderer.SetPropertyBlock(_propertyBlock, _matColorIndex);
        Color revealColor = ColorProvider.GetColor(_colorType);
        LerpColor(ColorProvider.HiddenColor, revealColor);
        UpdateTxt();
        IsHidden = false;

        UpdateLinkColor();

        OnOtherLinkVisible?.Invoke();
    }
    private void UpdateLinkColor()
    {
        if (_tempLink)
        {
            _tempLink.ApplyColorToFirstRendrer(!IsHidden ? ColorProvider.GetColor(_colorType) : _hiddenColor);
            _tempLink.ApplyColorToSecondRendrer(!_linked.IsHidden ? ColorProvider.GetColor(_linked._colorType) : _hiddenColor);
        }
    }
    private void OnDestroy()
    {
        if (_tempLink)
            Destroy(_tempLink.gameObject);
        _breathTween?.Kill();
        _colorTween?.Kill();
        _lerpTween?.Kill();
        _turretTween?.Kill();
        _shakeTween?.Kill();
        _cannonTween?.Kill();
        OnOtherLinkVisible = null;
        SignalBus.Unsubscribe<OnMagnetEnableSignal>(OnMagnetPowerUp);
    }

    private Tween _colorTween;
    private void LerpColor(Color from, Color to)
    {
        _colorTween?.Kill();

        Color tempColor = from;
        SetColor(tempColor);

        _colorTween = DOTween.To(() => tempColor, x =>
        {
            tempColor = x;
            SetColor(tempColor);
        },
        to, 0.5f)
        .SetEase(Ease.OutQuad);
    }

    private void OnMagnetPowerUp(OnMagnetEnableSignal signal)
    {
        if (IsLinked) return;
        if (IsHidden) return;
        DOVirtual.DelayedCall(signal.IsEnable ? 1 : 0, () => ReadyForMagnet(signal.IsEnable));
    }
    private void ReadyForMagnet(bool enable)
    {
        if (_canShoot) return;
        CanInterect = enable;
        ChangeTextAlpha(enable ? 1f : 0.3f);

        if (!enable)
        {
            if (IsFirstInRow)
                SetFirstInRow();
            else
                SetNotFirstInRow();
        }
    }

    private Tween _lerpTween;
    public void LerpColor(Color from, Color to, Action OnComplete = null)
    {
        _lerpTween?.Kill();

        Color tempColor = from;
        ApplyReflect(tempColor);

        _lerpTween = DOTween.To(() => tempColor, x =>
        {
            tempColor = x;
            ApplyReflect(tempColor);
        },
        to, 0.25f)
        .SetEase(Ease.OutQuad).OnComplete(() => OnComplete?.Invoke());
    }
}
public interface IColorLerpable
{
    public void LerpColor(Color from, Color to, Action OnComplete = null);
}