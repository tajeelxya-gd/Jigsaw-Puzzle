using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyController : MonoBehaviour, ITickable
{
    private LevelData levelData;

    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private EnemyGridSystem enemyGridSystem;
    public void Initialize(LevelData _levelData, ITutorial tutorialManager, IProgress progress)
    {
        levelData = _levelData;
        InitializePool();
        enemyGridSystem.Initialize(levelData, _enemyData, tutorialManager, progress);
        SignalBus.Subscribe<OnHammerEnableSignal>(OnHammerEnable);
        SignalBus.Subscribe<DestroyArmyThroughHammer>(DestroyArmyThroughHammer);
    }

    private bool _isPoolCreated = false;
    private void InitializePool()
    {
        if (_isPoolCreated) return;

        _isPoolCreated = true;
        int defCap = 0;
        int maxCap = 0;

        for (int i = 0; i < _enemyData.enemyInfos.Length; i++)
        {
            if (_enemyData.enemyInfos[i].gridItemType == GridItemType.StickMan)
            {
                defCap = 100;
                maxCap = 200;
            }
            else if (_enemyData.enemyInfos[i].gridItemType == GridItemType.Collectable)
            {
                defCap = 5;
                maxCap = 20;
            }
            else
            {
                defCap = 3;
                maxCap = 4;
            }

            switch (_enemyData.enemyInfos[i].enemyType)
            {
                case EnemyType.Simple: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<Stickman>(), defaultCapacity: defCap, maxSize: maxCap); break;
                case EnemyType.BulkyStickMan: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<MediumStickMan>(), defaultCapacity: defCap, maxSize: maxCap); break;
                case EnemyType.Tyre: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<Tyre>(), defaultCapacity: 5, 15); break;
                case EnemyType.Boss: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<BossLight>(), defaultCapacity: 2, 7); break;
                case EnemyType.Piggy: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<Piggy>(), defaultCapacity: 2, 7); break;
                case EnemyType.Key: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<Key>(), defaultCapacity: 2, 7); break;
                case EnemyType.Boggie: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<Boggie>(), defaultCapacity: 50, 100); break;
                case EnemyType.Box: PoolManager.CreatePool(_enemyData.enemyInfos[i].enemyPrefab.GetComponent<BoxArtillery>(), defaultCapacity: 2, 7); break;
                default: break;
            }
        }
    }

    public void Tick()
    {
        enemyGridSystem.Tick();
    }

    private Dictionary<ColorType, List<Enemy>> _colorGroupedEnemies = new();
    private bool _isHammerEnabled = false;
    private Tween _hammerBlinkTween;

    private void OnHammerEnable(OnHammerEnableSignal signal)
    {
        _isHammerEnabled = signal.IsEnable;

        StopBlinking();

        if (!_isHammerEnabled) return;

        DOVirtual.DelayedCall(1, () =>
        {
            RebuildColorGroups();
            StartBlinkingLoop();
        });
        // RebuildColorGroups();
        // StartBlinkingLoop();
    }
    private void RebuildColorGroups()
    {
        _colorGroupedEnemies.Clear();

        int columns = enemyGridSystem.GetColumnCount();

        for (int i = 0; i < columns; i++)
        {
            EnemyColumn column = enemyGridSystem.GetColumnAt(i);
            for (int j = 0; j < column.ColumnItemCount; j++)
            {
                if (column.GetItemAt(j) is Enemy e)
                {
                    if (e.EnemyType == EnemyType.Tyre || e.EnemyType == EnemyType.Piggy || e.EnemyType == EnemyType.Box
                    || e.EnemyType == EnemyType.Key || e.EnemyType == EnemyType.Boggie) continue;

                    if (!_colorGroupedEnemies.ContainsKey(e.ColorType))
                        _colorGroupedEnemies[e.ColorType] = new List<Enemy>();

                    _colorGroupedEnemies[e.ColorType].Add(e);
                }
            }
        }
    }
    private void StartBlinkingLoop()
    {
        StopBlinking();

        List<ColorType> colors = new List<ColorType>(_colorGroupedEnemies.Keys);

        if (colors.Count == 0) return;

        _hammerBlinkTween = DOVirtual.DelayedCall(0f, () =>
        {
            StartCoroutine(BlinkGroupsSequentially(colors));
        });
    }
    WaitForSeconds blinkDelay = new WaitForSeconds(1);
    private IEnumerator BlinkGroupsSequentially(List<ColorType> colors)
    {
        while (_isHammerEnabled)
        {
            foreach (var color in colors)
            {
                if (!_colorGroupedEnemies.ContainsKey(color)) continue;

                foreach (var e in _colorGroupedEnemies[color])
                    e.AnimateReflectOnce();

                yield return blinkDelay;
            }
        }
    }
    private void StopBlinking()
    {
        _hammerBlinkTween?.Kill();
        StopAllCoroutines();

        foreach (var colorGroup in _colorGroupedEnemies.Values)
            foreach (var enemy in colorGroup)
                enemy.ResetReflect();
    }

    private void DestroyArmyThroughHammer(DestroyArmyThroughHammer signal)
    {
        if (_colorGroupedEnemies.TryGetValue(signal.colorType, out var enemies))
        {
            int destroyedCount = 0;
            foreach (Enemy e in enemies.ToArray())
            {
                if (e != null)
                {
                    destroyedCount += e.Health;
                    e.Damage(e.Health);
                }
            }
            SignalBus.Publish(new OnCannonReInitiazeSignal { colorType = signal.colorType, AmountToReduce = destroyedCount });
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnHammerEnableSignal>(OnHammerEnable);
        SignalBus.Unsubscribe<DestroyArmyThroughHammer>(DestroyArmyThroughHammer);
    }
}