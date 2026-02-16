using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BoxArtillery : MonoBehaviour, IMoveable, IResumable, IRangeEvaluator, Initializable<BoxArtillery.BoxData>, IPoolable, IMoveableFast
{
    public bool CanMove { get; set; }

    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private EnemyConfig _enemyConfig;
    [SerializeField] private TextMeshPro _amountTxt;

    public bool isReached { get; set; }

    public BoxData _currentData;

    private bool _isReachedFlag;
    private bool _isDestined;

    private int _leftIndex = 0;
    private int _rightIndex = 0;

    private int _Remaining;

    float XPos;


    public void Initialize(params BoxArtillery.BoxData[] args)
    {
        if (args.Length <= 0) return;

        _currentData = args[0];
        SpeedCurve();

        Vector3 pos = transform.position;
        pos.x = (_currentData.leftColumn.transform.position.x + _currentData.rightColumn.transform.position.x) * 0.5f;
        XPos = pos.x;
        transform.position = pos;
        transform.localScale = Vector3.one;

        _Remaining = 0;
        _leftIndex = 0;
        _rightIndex = 0;
        _isDestined = false;
        isReached = false;

        for (int i = 0; i < _currentData.boxData.enemyData.Length; i++)
        {
            _Remaining += _currentData.boxData.enemyData[i].Enemies.Length;
        }

        CanMove = true;
        _isReachedFlag = false;

        _currentData.leftColumn.OnColumnEnemyDie += OnEnemyInMyColumnsDieLeft;
        _currentData.rightColumn.OnColumnEnemyDie += OnEnemyInMyColumnsDieRight;

        UpdateAmountText();
    }

    public void Move(Vector3 target)
    {
        if (!CanMove) return;

        CanMove = false;
        _isDestined = true;

        _navMeshAgent.SetDestination(new Vector3(XPos, target.y, target.z));
    }

    public void MoveFast(Vector3 target)
    {
        if (!CanMove) return;
        CanMove = false;
        // target.x = transform.position.x;
        _navMeshAgent.Warp(new Vector3(XPos, target.y, target.z));
        OnReached(false);
    }

    public bool CheckReached()
    {
        if (!_isDestined) return false;
        if (_navMeshAgent.pathPending) return false;
        if (_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance) return false;
        if (_navMeshAgent.velocity.sqrMagnitude > 0.01f) return false;

        return true;
    }

    public void OnReached(bool isFirst)
    {
        if (_isReachedFlag) return;
        _isReachedFlag = true;
        GenerateOnReach();
    }

    public void OnResume() { }

    private List<int> _rightColumnQueuedIndex = new List<int>();
    private List<int> _leftColumnQueuedIndex = new List<int>();

    private void GenerateOnReach()
    {
        if (_currentData.boxData.enemyData.Length < 2)
            return;

        var leftdata = _currentData.boxData.enemyData[1];
        for (int i = 0; i < _leftColumnQueuedIndex.Count; i++)
        {
            var info = leftdata.Enemies[_leftColumnQueuedIndex[i]];
            IMoveable moveable = _currentData.leftColumn.InsertEnemyAtIndex(info.enemyType, info.colorType, transform.position, _currentData.leftColumn.IndexOfItem(this),
            false, true, 3);
            _leftSpawnedEnemies.Add(moveable);

        }
        var rightdata = _currentData.boxData.enemyData[0];
        for (int i = 0; i < _rightColumnQueuedIndex.Count; i++)
        {
            var info = rightdata.Enemies[_rightColumnQueuedIndex[i]];
            IMoveable moveable = _currentData.rightColumn.InsertEnemyAtIndex(info.enemyType, info.colorType, transform.position, _currentData.rightColumn.IndexOfItem(this), false, true, 3);
            _rightSpawnedEnemies.Add(moveable);
        }

        _rightColumnQueuedIndex.Clear();
        _leftColumnQueuedIndex.Clear();
    }

    private HashSet<IMoveable> _leftSpawnedEnemies = new HashSet<IMoveable>();
    private HashSet<IMoveable> _rightSpawnedEnemies = new HashSet<IMoveable>();

    private void OnEnemyInMyColumnsDieLeft(IMoveable dead)
    {
        if (_Remaining <= 0) return;

        if (_currentData.leftColumn.HasMoveable(dead))
        {
            if (_currentData.leftColumn.IndexOfItem(dead) > _currentData.leftColumn.IndexOfItem(this))
                return;
        }
        if (!_isReachedFlag)
        {
            var data = _currentData.boxData.enemyData[1];
            if (_leftIndex >= data.Enemies.Length)
                return;

            _leftColumnQueuedIndex.Add(_leftIndex++);
            return;
        }

        SpawnNextEnemyLeft();

        if (!_leftSpawnedEnemies.Contains(dead))
            return;

        _leftSpawnedEnemies.Remove(dead);
        ReduceEnemy();
    }

    private void OnEnemyInMyColumnsDieRight(IMoveable dead)
    {
        if (_Remaining <= 0) return;

        if (_currentData.rightColumn.HasMoveable(dead))
        {
            if (_currentData.rightColumn.IndexOfItem(dead) > _currentData.rightColumn.IndexOfItem(this))
                return;
        }

        if (!_isReachedFlag)
        {
            var data = _currentData.boxData.enemyData[0];
            if (_rightIndex >= data.Enemies.Length)
                return;

            _rightColumnQueuedIndex.Add(_rightIndex++);
            return;
        }

        SpawnNextEnemyRight();

        if (!_rightSpawnedEnemies.Contains(dead))
            return;

        _rightSpawnedEnemies.Remove(dead);
        ReduceEnemy();
    }

    private void SpawnNextEnemyLeft()
    {
        var data = _currentData.boxData.enemyData[1];
        if (_leftIndex >= data.Enemies.Length)
            return;

        var info = data.Enemies[_leftIndex++];

        IMoveable spawned = _currentData.leftColumn.InsertEnemyAtIndex(
            info.enemyType,
            info.colorType,
            transform.position,
            _currentData.leftColumn.IndexOfItem(this),
            false,
            true,
            2
        );

        if (spawned != null)
            _leftSpawnedEnemies.Add(spawned);
    }

    private void SpawnNextEnemyRight()
    {
        var data = _currentData.boxData.enemyData[0];
        if (_rightIndex >= data.Enemies.Length)
            return;

        var info = data.Enemies[_rightIndex++];

        IMoveable spawned = _currentData.rightColumn.InsertEnemyAtIndex(
            info.enemyType,
            info.colorType,
            transform.position,
            _currentData.rightColumn.IndexOfItem(this),
            false,
            true,
            2
        );

        if (spawned != null)
            _rightSpawnedEnemies.Add(spawned);
    }

    private void ReduceEnemy()
    {
        _Remaining--;

        UpdateAmountText();

        if (_Remaining <= 0)
        {
            DestroyBox();
        }
    }

    private void DestroyBox()
    {
        OnDespawned();
        transform.DOScale(Vector3.one * 0.6f, 0.2f).SetEase(_enemyConfig.KillEffectCurve).OnComplete(() =>
        {
            _currentData.leftColumn.RemovePropID(_currentData.PropID);
            _currentData.rightColumn.RemovePropID(_currentData.PropID);

            _currentData.leftColumn.RemoveMoveable(this);
            _currentData.rightColumn.RemoveMoveable(this);

            GlobalService.ParticleService.PlayParticle(ParticleType.Hit, transform.position + (Vector3.up * 0.6f), true);
            PoolManager.GetPool<BoxArtillery>().Release(this);
        });
    }

    private void UpdateAmountText()
    {
        _amountTxt.text = _Remaining.ToString();
    }

    public void OnSpawned()
    {

    }

    public void OnDespawned()
    {
        if (_currentData != null)
        {
            _currentData.leftColumn.OnColumnEnemyDie -= OnEnemyInMyColumnsDieLeft;
            _currentData.rightColumn.OnColumnEnemyDie -= OnEnemyInMyColumnsDieRight;
        }
        _leftSpawnedEnemies.Clear();
        _rightSpawnedEnemies.Clear();
        _leftColumnQueuedIndex.Clear();
        _rightColumnQueuedIndex.Clear();
        _speedTween?.Kill();
    }

    private protected Tween _speedTween;
    private protected virtual void SpeedCurve()
    {
        _speedTween?.Kill();
        float startSpeed = _enemyConfig.Speed * 2;
        float endSpeed = _enemyConfig.Speed;
        float duration = 3f;

        _navMeshAgent.speed = startSpeed;

        _speedTween = DOTween.To(
                () => _navMeshAgent.speed,
                x => _navMeshAgent.speed = x,
                endSpeed,
                duration
            )
            .SetEase(_enemyConfig.SpeedCurve);
    }

    [System.Serializable]
    public class BoxData
    {
        public int PropID;
        public LevelData.BoxData boxData;
        public EnemyColumn leftColumn, rightColumn;
    }
}
public interface Initializable<T>
{
    public void Initialize(params T[] args);
}