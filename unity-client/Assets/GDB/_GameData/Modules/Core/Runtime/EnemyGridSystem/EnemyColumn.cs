using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Linq;
public class EnemyColumn : MonoBehaviour, ITickable
{
    public event System.Action<IMoveable> OnColumnEnemyDie;

    private const float JumpHeight = 2f;
    private EnemyData _enemyData;
    private readonly List<IMoveable> _moveables = new();
    private readonly Dictionary<BoggiePrewarmKey, Queue<Boggie>> _preWarmBoggies = new();
    private IAttackable _currentAttacker;
    public IMoveable GetItemAt(int index) => _moveables[index];
    public int IndexOfItem(IMoveable moveable) => _moveables.IndexOf(moveable);
    public int ColumnItemCount => _moveables.Count;

    private Coroutine _spawnRoutine;
    private EnemyGridSystem _enemyGridSystem;
    public IMoveable GetFirstMoveable() => _moveables[0];

    private ITutorial _iTutorial;
    public bool HasMoveable(IMoveable m)
    {
        return _moveables.Contains(m);
    }

    private readonly HashSet<int> _generatedBoxIDs = new();

    public void StartSpawning(EnemyGridSystem enemyGridSystem, EnemyData enemyPrefabData, LevelData.EnemyData enemyData, float spawnZOffset, float jumpTargetZ, float delay, ITutorial tutorialManager)
    {
        _iTutorial = tutorialManager;
        _enemyGridSystem = enemyGridSystem;
        _enemyData = enemyPrefabData;

        _canSpawn = true;

        _moveables.Clear();
        _generatedBoxIDs.Clear();
        _preWarmBoggies.Clear();
        _currentAttacker = null;

        SignalBus.Subscribe<OnEnemyDieSignal>(OnEnemyDie);
        SignalBus.Subscribe<MediumStickMan.OnSpawnEnemyAtSignal>(OnSpawnEnemyAt);
        SignalBus.Subscribe<OnlevelFailSignal>(OnLevelFail);
        SignalBus.Subscribe<OnRevivalSignal>(OnRevivalSignal);
        SignalBus.Subscribe<OnWallRevivalSignal>(OnRevivalSignal);

        SpawnBoggies(enemyData, spawnZOffset);

        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        _spawnRoutine = StartCoroutine(SpawnEnemiesRoutine(enemyData, spawnZOffset, delay));
    }

    private void SpawnBoggies(LevelData.EnemyData enemyData, float spawnZOffset)
    {
        List<Boggie> boggies = new List<Boggie>();
        for (int i = 0; i < enemyData.enemyColumns.Length; i++)
        {
            var info = enemyData.enemyColumns[i];

            if (info.enemyType != EnemyType.Boggie)
                continue;

            GameObject go = GetEnemy(EnemyType.Boggie);
            if (go == null) continue;

            go.SetActive(false);

            if (!go.TryGetComponent(out Boggie boggie))
                continue;

            LevelData.BoggieData data =
                _enemyGridSystem.GetBooggieData(info.PropID);

            boggie.InitializeBoggie(
                info.PropID,
                info.subPropID,
                data.colorType
            );

            var key = new BoggiePrewarmKey(i, info.PropID, data.colorType);

            if (!_preWarmBoggies.TryGetValue(key, out var queue))
            {
                queue = new Queue<Boggie>();
                _preWarmBoggies.Add(key, queue);
            }

            queue.Enqueue(boggie);

            boggies.Add(boggie);
        }
        DOVirtual.DelayedCall(.5f, () =>
        {
            for (int i = 0; i < boggies.Count; i++)
            {
                boggies[i].OnSpawn();
            }
        });
    }

    private bool _canSpawn = false;

    private IEnumerator SpawnEnemiesRoutine(LevelData.EnemyData enemyData, float spawnZOffset, float delay)
    {
        WaitForSeconds wait = new WaitForSeconds(delay);

        for (int i = 0; i < enemyData.enemyColumns.Length; i++)
        {
            yield return new WaitUntil(() => _canSpawn == true);
            if (ColumnItemCount > 16)
                yield return new WaitUntil(() => ColumnItemCount < 16);

            var info = enemyData.enemyColumns[i];

            if (_generatedBoxIDs.Contains(info.PropID))
            {
                continue;
            }

            GameObject enemy = null;

            if (info.enemyType == EnemyType.Boggie)
            {
                var key = new BoggiePrewarmKey(
                    i,
                    info.PropID,
                    info.colorType
                );

                if (_preWarmBoggies.TryGetValue(key, out var queue) &&
                    queue.Count > 0)
                {
                    Boggie boggie = queue.Dequeue();
                    if (boggie.IsHalted)
                    {
                        enemy = null;
                        _enemyGridSystem.OnEnemyKill();
                        yield return wait;
                        continue;
                    }
                    else
                    {
                        enemy = boggie.gameObject;
                        enemy.SetActive(true);

                        if (queue.Count == 0)
                            _preWarmBoggies.Remove(key);

                        Vector3 spawnPos = transform.position + new Vector3(0, 0, spawnZOffset);
                        enemy.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
                    }
                }
            }

            if (enemy == null && info.enemyType != EnemyType.Boggie)
            {
                enemy = SpawnEnemy(info.enemyType, spawnZOffset, 0);
            }

            if (enemy == null)
                continue;

            if (enemy.TryGetComponent(out Enemy component))
            {
                component.Initialize(info.colorType, info.EnemyHealth);
                component.OnResume();
            }

            if (enemy.TryGetComponent(out BoxArtillery artillery))
            {
                if (_generatedBoxIDs.Contains(info.PropID))
                {
                    yield return wait;
                    continue;
                }

                AddMoveable(enemy.GetComponent<IMoveable>());

                _generatedBoxIDs.Add(info.PropID);

                LevelData.BoxData boxData = _enemyGridSystem.GetBoxData(info.PropID);
                int myIndex = _enemyGridSystem.GetColumnIndex(this);
                int leftColumnIndex = Mathf.Min(boxData.ColumnID, boxData.SecondColumnID);
                int rightColumnIndex = Mathf.Max(boxData.ColumnID, boxData.SecondColumnID);
                EnemyColumn leftColumn = _enemyGridSystem.GetColumnAt(leftColumnIndex);
                EnemyColumn rightColumn = _enemyGridSystem.GetColumnAt(rightColumnIndex);

                if (myIndex == leftColumnIndex)
                {
                    rightColumn.AddPropID(info.PropID);
                    rightColumn.AddMoveable(artillery);
                }
                else
                {
                    leftColumn.AddPropID(info.PropID);
                    leftColumn.AddMoveable(artillery);
                }

                artillery.Initialize(new BoxArtillery.BoxData()
                {
                    PropID = boxData.PropID,
                    boxData = boxData,
                    leftColumn = leftColumn,
                    rightColumn = rightColumn
                });

                yield return wait;
                continue;
            }

            AddMoveable(enemy.GetComponent<IMoveable>());
            StartCoroutine(JumpAndMove(enemy.transform, -10f));

            yield return wait;
        }
    }

    private GameObject SpawnEnemy(EnemyType type, float spawnZOffset, float xOffset = 0)
    {
        Vector3 spawnPos = transform.position + new Vector3(xOffset, 0, spawnZOffset);
        GameObject enemy = null;

        enemy = GetEnemy(type);

        if (enemy == null) return null;

        enemy.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        return enemy.gameObject;
    }

    private GameObject GetEnemy(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Simple: return PoolManager.GetPool<Stickman>().Get().gameObject;
            case EnemyType.BulkyStickMan: return PoolManager.GetPool<MediumStickMan>().Get().gameObject;
            case EnemyType.Tyre: return PoolManager.GetPool<Tyre>().Get().gameObject;
            case EnemyType.Box: return PoolManager.GetPool<BoxArtillery>().Get().gameObject;
            case EnemyType.Boss: return PoolManager.GetPool<BossLight>().Get().gameObject;
            case EnemyType.Piggy: return PoolManager.GetPool<Piggy>().Get().gameObject;
            case EnemyType.Key: return PoolManager.GetPool<Key>().Get().gameObject;
            case EnemyType.Boggie: return PoolManager.GetPool<Boggie>().Get().gameObject;
            default: break;
        }
        return null;
    }

    private IEnumerator JumpAndMove(Transform enemy, float targetZ)
    {
        float jumpDuration = 0.5f;

        Vector3 jumpTarget = new(enemy.position.x, enemy.position.y, targetZ);

        enemy.transform.position = jumpTarget;

        Sequence seq = DOTween.Sequence()
      .Append(enemy.transform
          .DOScale(Vector3.one, jumpDuration)
          .From(Vector3.one * 0.5f)
          .SetEase(Ease.OutBack)
      );

        yield return null;
    }

    public void AddMoveable(IMoveable moveable)
    {
        if (!_moveables.Contains(moveable))
        {
            _moveables.Add(moveable);

            if (moveable is IResumable enemy)
            {
                enemy.OnResume();
            }
        }
    }

    public void RemoveMoveable(IMoveable moveable, bool _isSpecial = false)
    {
        if (_moveables.Contains(moveable))
        {
            _moveables.Remove(moveable);
            ResetEnemies();
            if (!_isSpecial)
                _enemyGridSystem.OnEnemyKill();
        }
    }

    private void AssignAttacker(IAttackable attackable)
    {
        _currentAttacker = attackable;
    }

    private void RemoveAttacker(IAttackable attackable)
    {
        if (_currentAttacker == attackable)
        {
            _currentAttacker = null;
        }
    }

    private void ResetEnemies()
    {
        for (int i = 0; i < _moveables.Count; i++)
        {
            if (!_moveables[0].CanMove) return;
            if (_moveables[0] is Enemy enemy)
                if (enemy.isLocked) return;

            IMoveable moveable = _moveables[i];
            if (moveable is IResumable resumable)
            {
                resumable.OnResume();
                Tick();
            }
        }
    }

    private void OnSpawnEnemyAt(MediumStickMan.OnSpawnEnemyAtSignal signal)
    {
        ReplaceEnemyAtIndex(signal.enemyType, signal.colorType, _moveables.IndexOf(signal.moveable));
    }

    private void ReplaceEnemyAtIndex(EnemyType typeToSpawn, ColorType colorType, int index)
    {
        if (index < 0 || index >= _moveables.Count)
            return;

        IMoveable oldMoveable = _moveables[index];
        Transform oldTransform = (oldMoveable as MonoBehaviour).transform;

        Vector3 spawnPos = oldTransform.position;
        Quaternion spawnRot = oldTransform.rotation;

        _moveables.RemoveAt(index);

        GameObject newEnemy = GetEnemy(typeToSpawn);
        if (newEnemy == null) return;

        if (newEnemy.TryGetComponent(out Enemy component))
        {
            component.Initialize(colorType, 1);
            component.OnResume();
        }

        newEnemy.transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (newEnemy.TryGetComponent(out IMoveable newMoveable))
        {
            _moveables.Insert(index, newMoveable);
        }

        ResetEnemies();
    }

    public IMoveable InsertEnemyAtIndex(EnemyType typeToSpawn, ColorType colorType, Vector3 Pos, int index, bool CanAvoid = true, bool OverideSpeed = false, float Speed = 1)
    {
        if (index < 0) index = 0;
        if (index > _moveables.Count) index = _moveables.Count;

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (_moveables.Count > 0 && index < _moveables.Count)
        {
            if (_moveables[index] is MonoBehaviour refMB)
            {
                spawnPos = Pos;
                spawnPos.x = transform.position.x;
                spawnPos.z = Pos.z + 0.5f;
            }
        }

        GameObject enemyGO = GetEnemy(typeToSpawn);
        if (enemyGO == null) return null;

        enemyGO.transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (enemyGO.TryGetComponent(out Enemy enemy))
        {
            enemy.Initialize(colorType, 1, OverideSpeed, Speed);
            enemy.OnResume();

            if (!CanAvoid)
                enemy.NavMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        IMoveable moveable = null;

        if (enemyGO.TryGetComponent(out moveable))
            _moveables.Insert(index, moveable);

        ResetEnemies();

        return moveable;
    }

    public void Tick()
    {
        for (int i = 0; i < _moveables.Count; i++)
        {
            IMoveable moveable = _moveables[i];

            if (!moveable.CanMove)
            {
                if (moveable is IRangeEvaluator range)
                {
                    if (range.isReached)
                        continue;

                    if (range.CheckReached())
                    {
                        range.OnReached(i == 0);
                    }
                }
                continue;
            }

            Vector3 target = transform.position;
            target.z -= i * 0.5f;

            if (_iTutorial != null && _iTutorial.HasFastInitialization())
            {
                if (moveable is IMoveableFast fast)
                    fast.MoveFast(target);
                else
                    moveable.Move(target);
            }
            else
                moveable.Move(target);


            if (moveable is IRangeEvaluator enemy)
            {
                if (enemy.isReached)
                    continue;

                if (enemy.CheckReached())
                {
                    enemy.OnReached(i == 0);
                    if (i == 0)
                        AssignAttacker(enemy as IAttackable);
                }
            }
        }

        if (_currentAttacker != null)
            _currentAttacker.Attack();
    }

    private void OnEnemyDie(OnEnemyDieSignal signal)
    {
        IMoveable dead = signal.enemy;

        if (_moveables.Contains(dead))
        {
            OnColumnEnemyDie?.Invoke(dead);
            RemoveMoveable(dead, signal.IsSpecial);

            if (_moveables.Count > 0 && GetFirstMoveable() is IAutoCollectable autoCollectable)
                autoCollectable.AutoCollect();
        }
        if (signal.enemy is IAttackable attackable)
            RemoveAttacker(attackable);
    }

    public void AddPropID(int ID)
    {
        if (!_generatedBoxIDs.Contains(ID))
            _generatedBoxIDs.Add(ID);
    }
    public void RemovePropID(int ID)
    {
        if (_generatedBoxIDs.Contains(ID))
            _generatedBoxIDs.Remove(ID);
    }

    private void OnLevelFail(ISignal signal)
    {
        _canSpawn = false;
        // if (_spawnRoutine != null)
        //     StopCoroutine(_spawnRoutine);
    }

    private void OnRevivalSignal(ISignal signal)
    {
        _canSpawn = true;
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnlevelFailSignal>(OnLevelFail);
        SignalBus.Unsubscribe<OnEnemyDieSignal>(OnEnemyDie);
        SignalBus.Unsubscribe<MediumStickMan.OnSpawnEnemyAtSignal>(OnSpawnEnemyAt);
        SignalBus.Unsubscribe<OnRevivalSignal>(OnRevivalSignal);
        SignalBus.Unsubscribe<OnWallRevivalSignal>(OnRevivalSignal);
    }
}