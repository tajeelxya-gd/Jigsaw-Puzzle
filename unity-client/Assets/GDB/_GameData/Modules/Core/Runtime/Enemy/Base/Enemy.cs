using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable, IMoveable, IMoveableFast, IPoolable, IResumable, IRangeEvaluator, IInterectable
{
    private protected readonly int AnimIndexID = Shader.PropertyToID("_AnimIndex");
    private protected readonly int AnimSpeedID = Shader.PropertyToID("_AnimLength");

    public bool CanMove { get; set; }

    public bool CanLockedAgain
    {
        get
        {
            return _canLookedMultipleTimes;
        }
    }
    private protected bool _canLookedMultipleTimes = false;

    public bool isLocked { get; set; }
    public bool isReached { get; set; }
    public ColorType ColorType { get; protected set; }
    public int Health { get; protected set; }
    public bool CanInterect { get; set; }

    [SerializeField] private protected EnemyType _enemyType;
    public EnemyType EnemyType { get { return _enemyType; } }
    [SerializeField] private protected EnemyConfig _enemyConfig;
    [SerializeField] private protected bool _useShaderAnim = false;
    [SerializeField][ShowIf("@_useShaderAnim == true")] private int _idleAnimNumber = 0;
    [SerializeField][ShowIf("@_useShaderAnim == true")] private int _runAnimNumber = 1;
    [SerializeField][ShowIf("@_useShaderAnim == false")] private protected Animator _animator;
    public NavMeshAgent NavMeshAgent { get { return _navMeshAgent; } }
    [SerializeField] private protected NavMeshAgent _navMeshAgent;
    [SerializeField] private protected Renderer[] _renderer;

    [SerializeField] private protected bool UseMaterial = false;
    public virtual void Initialize(ColorType colorType, int health = 1, bool OverideSpeed = false, float Speed = 1)
    {
        if (_navMeshAgent.obstacleAvoidanceType == ObstacleAvoidanceType.NoObstacleAvoidance)
            _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        if (!UseMaterial)
            SetColor(ColorProvider.GetColor(colorType));
        else
        {
            _renderer[0].sharedMaterial = ColorProvider.GetMaterial(colorType, _enemyType);
            SetShaderAnimSpeed(1);
        }
        if (OverideSpeed)
            _navMeshAgent.speed = Speed;
        else
            SpeedCurve();
        // _navMeshAgent.speed = _enemyConfig.Speed;
        Health = health;
        CanMove = true;
        // _navMeshAgent.updatePosition = false;
        //   _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }
    public abstract void OnBulletLock();
    public abstract void Damage(float val);
    public abstract void OnSpawned();
    public abstract void OnDespawned();

    private protected bool _isDestined;
    public virtual void Move(Vector3 target)
    {
        transform.position = new Vector3(target.x, transform.position.y, transform.position.z);
        if (_isDestined) return;
        if (!gameObject.activeSelf) return;
        if (!_navMeshAgent.enabled) return;

        target.x = transform.position.x;
        _navMeshAgent.SetDestination(target);
        _isDestined = true;
    }
    public void MoveFast(Vector3 target)
    {
        if (_isDestined) return;
        if (!gameObject.activeSelf) return;
        if (!_navMeshAgent.enabled) return;
        target.x = transform.position.x;
        _navMeshAgent.Warp(target);
        _isDestined = true;
    }

    public virtual void OnResume()
    {
        _isDestined = false;
        isReached = false;
        isLocked = false;
        _navMeshAgent.enabled = true;
        OnRun();
    }
    public virtual void OnReached(bool isFirst)
    {
        isReached = true;
        _navMeshAgent.enabled = false;

        if (transform.eulerAngles.y != 0)
        {
            transform.eulerAngles = Vector3.zero;
        }
        if (!isFirst)
            OnPause();
    }
    public virtual bool CheckReached()
    {
        if (!_isDestined) return false;

        if (!gameObject.activeSelf)
            return false;

        if (_navMeshAgent.pathPending)
            return false;

        if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance)
            return false;

        if (_navMeshAgent.hasPath && _navMeshAgent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
    }
    private MaterialPropertyBlock[] _blocks;

    public virtual void SetColor(Color color)
    {
        if (_blocks == null)
            CreateBlocks();

        for (int i = 0; i < _renderer.Length; i++)
        {
            _blocks[i].SetColor("_BaseColor", color);
            _renderer[i].SetPropertyBlock(_blocks[i]);
        }
    }
    private void CreateBlocks()
    {
        _blocks = new MaterialPropertyBlock[_renderer.Length];
        for (int i = 0; i < _renderer.Length; i++)
        {
            _blocks[i] = new MaterialPropertyBlock();
            _renderer[i].GetPropertyBlock(_blocks[i]);
        }
    }

    public virtual void AnimateReflectOnce()
    {
        LerpColor(Color.black, Color.gray8, () => LerpColor(Color.gray8, Color.black));
    }
    private protected void ApplyReflect(Color color)
    {
        for (int i = 0; i < _renderer.Length; i++)
        {
            _blocks[i].SetColor("_EmissionColor", color);
            _renderer[i].SetPropertyBlock(_blocks[i]);
        }
    }
    public virtual void ResetReflect()
    {
        _colorTween?.Kill();
        ApplyReflect(Color.black);
    }
    private Tween _colorTween;
    private void LerpColor(Color from, Color to, Action OnComplete = null)
    {
        _colorTween?.Kill();

        Color tempColor = from;
        ApplyReflect(tempColor);

        _colorTween = DOTween.To(() => tempColor, x =>
        {
            tempColor = x;
            ApplyReflect(tempColor);
        },
        to, 0.5f)
        .SetEase(Ease.OutQuad).OnComplete(() => OnComplete?.Invoke());
    }
    private protected virtual void OnPause()
    {
        if (_useShaderAnim)
        {
            PlayShaderAnim(_idleAnimNumber);
            return;
        }
        if (_animator)
            _animator.SetTrigger("Idle");
    }
    private protected virtual void OnRun()
    {
        if (_useShaderAnim)
        {
            PlayShaderAnim(_runAnimNumber);
            return;
        }
        if (_animator)
            _animator.SetTrigger("Run");
    }

    private protected void PlayShaderAnim(float id)
    {
        if (_blocks == null)
            CreateBlocks();

        for (int i = 0; i < _renderer.Length; i++)
        {
            _blocks[i].SetFloat(AnimIndexID, id);
            _renderer[i].SetPropertyBlock(_blocks[i]);
        }
    }
    private protected void SetShaderAnimSpeed(float speed)
    {
        if (_blocks == null)
            CreateBlocks();
        for (int i = 0; i < _renderer.Length; i++)
        {
            _blocks[i].SetFloat(AnimSpeedID, speed);
            _renderer[i].SetPropertyBlock(_blocks[i]);
        }
    }
    private protected Tween _speedTween;
    private protected virtual void SpeedCurve()
    {
        _speedTween?.Kill();
        float startSpeed = _enemyConfig.Speed * 2;
        float endSpeed = _enemyConfig.Speed;
        float duration = 5f;

        _navMeshAgent.speed = startSpeed;

        _speedTween = DOTween.To(
                () => _navMeshAgent.speed,
                x => _navMeshAgent.speed = x,
                endSpeed,
                duration
            )
            .SetEase(_enemyConfig.SpeedCurve);
    }
}