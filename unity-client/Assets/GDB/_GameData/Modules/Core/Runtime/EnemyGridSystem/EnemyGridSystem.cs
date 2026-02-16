using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGridSystem : MonoBehaviour, IGrid, IEnemyProvider
{
    [Header("Layout Settings")]
    [SerializeField][Range(0f, 5f)] private float _columnSpacing = 2f;
    [SerializeField] private Transform _spawnRoot;
    [SerializeField][Range(-5f, 5f)] private float columnPosOffset;

    [Header("Spawn Settings")]
    [SerializeField][Range(-10f, 10f)] private float _zSpawnOffset = -6f;
    [SerializeField][Range(-10f, 10f)] private float _jumpTargetZ = -2f;

    public EnemyColumn GetColumnAt(int index) => _columns[index];
    public int GetColumnIndex(EnemyColumn column) => _columns.IndexOf(column);

    private readonly List<EnemyColumn> _columns = new();
    private LevelData _levelData;
    private EnemyData _enemyData;

    public int TotalEnemies { get; private set; }
    public int RemainingEnemies { get; private set; }

    private HashSet<ITickable> _tickables = new HashSet<ITickable>();
    private ITutorial iTutorial;
    private IProgress _progressTracker;

    public void Initialize(LevelData levelData, EnemyData enemyData, ITutorial tutorialManager, IProgress progress)
    {
        iTutorial = tutorialManager;
        _levelData = levelData;
        _enemyData = enemyData;
        _progressTracker = progress;

        TotalEnemies = 0;

        CreateColumns(levelData.enemyData.Length, _columnSpacing);

        for (int i = 0; i < _columns.Count; i++)
        {
            LevelData.EnemyData data = _levelData.enemyData[i];
            _columns[i].StartSpawning(this, _enemyData, data, _zSpawnOffset, _jumpTargetZ, iTutorial.HasFastInitialization() ? Time.deltaTime : 0.5f, iTutorial);
            for (int J = 0; J < data.enemyColumns.Length; J++)
            {
                if (data.enemyColumns[J].enemyType != EnemyType.Piggy)
                    TotalEnemies += 1;
            }
            // TotalEnemies += data.enemyColumns.Length;
        }

        if (_levelData.boxData != null)
        {
            for (int i = 0; i < _levelData.boxData.Length; i++)
            {
                for (int J = 0; J < _levelData.boxData[i].enemyData.Length; J++)
                {
                    TotalEnemies += _levelData.boxData[i].enemyData[J].Enemies.Length;
                }
            }
        }

        RemainingEnemies = TotalEnemies;
    }

    public void CreateColumns(int count, float distance)
    {
        foreach (var col in _columns)
            Destroy(col.gameObject);
        _columns.Clear();
        _tickables.Clear();

        float startX = -((count - 1) * distance) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new(startX + i * distance, 0f, 0f);

            GameObject colObj = new GameObject($"EnemyColumn_{i}");
            colObj.transform.SetParent(_spawnRoot, false);
            colObj.transform.localPosition = pos + new Vector3(0, 0, columnPosOffset);

            EnemyColumn col = colObj.AddComponent<EnemyColumn>();
            _columns.Add(col);
            _tickables.Add(col);
        }
    }

    public void AddItemInColumn<T>(T item, int index)
    {
        if (index < 0 || index >= _columns.Count) return;
        if (item is IMoveable moveable)
            _columns[index].AddMoveable(moveable);
    }

    public Enemy ProvideEnemy(int _lastCheckedColumnIndex, ColorType colorType, Vector3 origin, float Range)
    {
        int totalColumns = _columns.Count;

        for (int i = 0; i < totalColumns; i++)
        {
            int colIndex = (_lastCheckedColumnIndex + i) % totalColumns;
            var column = _columns[colIndex];

            if (column.ColumnItemCount > 0)
            {
                if (column.GetFirstMoveable() is IAutoCollectable) continue;
                Enemy enemy = column.GetFirstMoveable() as Enemy;
                if (enemy != null && (!enemy.isLocked || enemy.CanLockedAgain))
                {
                    if (enemy.ColorType == colorType || enemy is ICollectable)
                    {
                        float distance = Vector3.Distance(Vector3.zero, enemy.transform.position);
                        if (distance <= Range)
                        {
                            enemy.OnBulletLock();
                            return enemy;
                        }
                    }
                }
            }
        }
        return null;
    }
    public int GetColumnCount() => _columns.Count;
    public bool IsFirst(IMoveable moveable)
    {
        for (int i = 0; i < _columns.Count; i++)
        {
            if (_columns[i].ColumnItemCount <= 0) continue;
            if (_columns[i].GetFirstMoveable() == moveable) return true;
        }
        return false;
    }

    public void Tick()
    {
        foreach (var Tick in _tickables)
        {
            Tick.Tick();
        }
    }
    public LevelData.BoxData GetBoxData(int prop)
    {
        for (int i = 0; i < _levelData.boxData.Length; i++)
        {
            if (_levelData.boxData[i].PropID == prop)
                return _levelData.boxData[i];
        }
        return null;
    }
    public LevelData.BoggieData GetBooggieData(int prop)
    {
        for (int i = 0; i < _levelData.boggiesData.Length; i++)
        {
            if (_levelData.boggiesData[i].BoggieID == prop)
                return _levelData.boggiesData[i];
        }
        return null;
    }

    private float _progressVal;
    public void OnEnemyKill()
    {
        RemainingEnemies--;

        _progressVal = RemainingEnemies / (float)TotalEnemies;

        _progressTracker.UpdateProgress(1 - _progressVal);
        if (RemainingEnemies <= 0)
        {
            SignalBus.Publish(new OnLevelCompleteSignal { levelType = _levelData.levelType });
        }
    }
}