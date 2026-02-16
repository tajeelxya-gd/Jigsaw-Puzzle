using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

public class CannonGridSystem : MonoBehaviour, IGrid
{
    [SerializeField, Min(0.1f)] private float _columnSpacing = 2f;
    [SerializeField, Min(0.1f)] private float _itemSpacing = 1.5f;
    [SerializeField] private float _columnZOffset = 0f;

    [SerializeField] private List<ColumnData> _columns = new();

    [System.Serializable]
    private class ColumnData
    {
        public Transform columnTransform;
        public List<Cannon> cannons = new();
    }

    private void Start()
    {
        SignalBus.Subscribe<OnShooterRemoveFromGridSignal>(OnShootableRemove);
        SignalBus.Subscribe<OnCannonReInitiazeSignal>(OnCannonReInitiaze);
        SignalBus.Subscribe<OnLevelCompleteSignal>(OnLevelComplete);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnShooterRemoveFromGridSignal>(OnShootableRemove);
        SignalBus.Unsubscribe<OnCannonReInitiazeSignal>(OnCannonReInitiaze);
        SignalBus.Unsubscribe<OnLevelCompleteSignal>(OnLevelComplete);
    }

    public void CreateColumns(int count, float distance)
    {
        _columnSpacing = distance;

        foreach (var c in _columns)
        {
            if (c.columnTransform != null)
                Destroy(c.columnTransform.gameObject);

            for (int i = 0; i < c.cannons.Count; i++)
            {
                if (c.cannons[i] != null)
                    Destroy(c.cannons[i].gameObject);
            }
        }

        _columns.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject column = new($"CannonColumn_{i + 1}");
            column.transform.SetParent(transform);

            _columns.Add(new ColumnData
            {
                columnTransform = column.transform,
                cannons = new List<Cannon>()
            });
        }
        _columns.Reverse();
        AlignColumns();
    }

    public void AddItemInColumn<T>(T item, int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count)
        {
            return;
        }

        if (item is Cannon component)
        {
            ColumnData colData = _columns[columnIndex];
            Transform column = colData.columnTransform;

            Transform itemTransform = component.transform;
            itemTransform.SetParent(column);
            colData.cannons.Add(component);

            AlignItemsInColumn(colData, false, columnIndex);
        }
    }

    private void AlignColumns()
    {
        if (_columns.Count == 0) return;

        float totalWidth = (_columns.Count - 1) * _columnSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < _columns.Count; i++)
        {
            Transform col = _columns[i].columnTransform;
            if (col == null) continue;

            float xPos = startX + i * _columnSpacing;
            float zPos = _columnZOffset;

            col.localPosition = new Vector3(xPos, 0f, zPos);
            AlignItemsInColumn(_columns[i], false, i);
        }
    }

    private void AlignItemsInColumn(ColumnData column, bool animate = false, int id = 0)
    {
        for (int i = 0; i < column.cannons.Count; i++)
        {
            if (id >= 0)
                column.cannons[i].ColumnId = id;
            Transform item = column.cannons[i].transform;
            if (item == null) continue;

            Vector3 targetLocalPos = new Vector3(0f, 0f, i * _itemSpacing);

            if (animate)
            {
                item.DOKill();
                item.DOLocalMove(targetLocalPos, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(i * 0.1f);
            }
            else
            {
                item.localPosition = targetLocalPos;
            }

            if (i == 0)
                column.cannons[i].SetFirstInRow();
            else
                column.cannons[i].SetNotFirstInRow();
        }
    }

    private Tween _timeSpeedTween;

    private void OnShootableRemove(OnShooterRemoveFromGridSignal signal)
    {
        if (signal == null || signal.shootable == null || signal.shootable.Count == 0)
            return;

        HashSet<ColumnData> affectedColumns = new HashSet<ColumnData>();

        foreach (var shootableObj in signal.shootable)
        {
            Cannon shootableCannon = shootableObj as Cannon;
            if (shootableCannon == null) continue;

            foreach (var column in _columns)
            {
                if (column.cannons.Contains(shootableCannon))
                {
                    column.cannons.Remove(shootableCannon);
                    affectedColumns.Add(column);
                    break;
                }
            }
            shootableCannon.InitializeForShoot();
        }

        foreach (var column in affectedColumns)
        {
            AlignItemsInColumn(column, animate: true, -1);
        }

        if (_columns.All(c => c.cannons.Count == 0))
        {
            _timeSpeedTween = DOVirtual.Float(1f, 2f, 3f, val => Time.timeScale = val);
        }
    }

    private void OnLevelComplete(ISignal signal)
    {
        _timeSpeedTween?.Kill();
    }

    [Button]
    public void RefreshLayout()
    {
        AlignColumns();
    }

    [Button]
    public void Shuffle()
    {
        List<Cannon> freeCannons = new();
        Dictionary<ColumnData, List<int>> freeSlots = new();

        foreach (var col in _columns)
        {
            freeSlots[col] = new List<int>();

            for (int i = 0; i < col.cannons.Count; i++)
            {
                Cannon c = col.cannons[i];

                bool isLocked =
                    c._cannonType == ShooterType.LockedCannon ||
                    (c is ILink<Cannon> link && link.IsLinked);

                if (isLocked)
                    continue;

                freeCannons.Add(c);
                freeSlots[col].Add(i);

                col.cannons[i] = null;
            }
        }

        if (freeCannons.Count <= 1)
            return;

        for (int i = 0; i < freeCannons.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, freeCannons.Count);
            (freeCannons[i], freeCannons[r]) = (freeCannons[r], freeCannons[i]);
        }

        int freeIndex = 0;

        for (int colIndex = 0; colIndex < _columns.Count; colIndex++)
        {
            ColumnData col = _columns[colIndex];

            foreach (int slot in freeSlots[col])
            {
                Cannon cannon = freeCannons[freeIndex++];

                col.cannons[slot] = cannon;
                cannon.ColumnId = colIndex;
                cannon.transform.SetParent(col.columnTransform, true);
            }
        }

        AnimateColumnShuffle();
    }
    private void AnimateColumnShuffle()
    {
        float outwardDistance = 0.7f;
        float duration = 0.25f;

        for (int colIndex = 0; colIndex < _columns.Count; colIndex++)
        {
            ColumnData column = _columns[colIndex];

            for (int row = 0; row < column.cannons.Count; row++)
            {
                Cannon cannon = column.cannons[row];
                if (cannon == null)
                    continue;

                cannon.ColumnId = colIndex;

                bool isLocked =
                    cannon._cannonType == ShooterType.LockedCannon ||
                    (cannon is ILink<Cannon> link && link.IsLinked);

                Vector3 finalLocalPos = new Vector3(0f, 0f, row * _itemSpacing);

                if (isLocked)
                {
                    continue;
                }

                int rowIndex = row;

                Vector3 scatterDir = UnityEngine.Random.insideUnitSphere;
                scatterDir.y = 0f;
                scatterDir.Normalize();

                Vector3 scatterTarget = finalLocalPos + scatterDir * outwardDistance;

                cannon.transform.DOKill();
                cannon.transform
                    .DOLocalMove(scatterTarget, duration * 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        cannon.transform
                            .DOLocalMove(finalLocalPos, duration)
                            .SetEase(Ease.OutBack)
                            .OnComplete(() =>
                            {
                                if (rowIndex == 0)
                                {
                                    cannon.SetFirstInRow();
                                    cannon.CanInterect = true;
                                }
                                else
                                {
                                    cannon.SetNotFirstInRow();
                                    cannon.CanInterect = false;
                                }
                            });
                    });
            }
        }
    }

    private void OnCannonReInitiaze(OnCannonReInitiazeSignal signal)
    {
        ReduceProjectiles(signal.colorType, signal.AmountToReduce);
    }

    private void ReduceProjectiles(ColorType color, int amountToReduce)
    {
        for (int colIndex = 0; colIndex < _columns.Count; colIndex++)
        {
            var column = _columns[colIndex];

            for (int i = column.cannons.Count - 1; i >= 0 && amountToReduce > 0; i--)
            {
                Cannon cannon = column.cannons[i];
                if (cannon._colorType != color) continue;

                int remaining = cannon.TotalProjectileRemaining;
                int reduce = Mathf.Min(remaining, amountToReduce);

                cannon.UpdateProjectile(remaining - reduce);
                amountToReduce -= reduce;

                if (cannon.TotalProjectileRemaining <= 0)
                {
                    column.cannons.RemoveAt(i);
                    Destroy(cannon.gameObject);
                }
            }
        }

        foreach (var column in _columns)
            AlignItemsInColumn(column, animate: true);

        if (amountToReduce > 0)
            SignalBus.Publish(new OnSpaceShooterCorrectionSignal { colorType = color, AmountToReduce = amountToReduce });
    }
    public List<IShootable> GetShootablesForSlotPopper()
    {
        List<IShootable> shootables = new List<IShootable>
        {
            _columns[0].cannons[0],
            _columns[1].cannons[0],
            _columns[2].cannons[0],
            _columns[2].cannons[1]
        };
        return shootables;
    }
    public IShootable GetInterectableShooterForMagnetOnBoard()
    {
        return _columns[1].cannons[2];
    }

    public bool HasAnyCannon()
    {
        if (_columns == null || _columns.Count == 0)
            return false;

        for (int col = 0; col < _columns.Count; col++)
        {
            var column = _columns[col];
            if (column == null || column.cannons == null)
                continue;

            for (int i = 0; i < column.cannons.Count; i++)
            {
                if (column.cannons[i] != null)
                    return true;
            }
        }

        return false;
    }
}