using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SpaceController : MonoBehaviour, ITickable
{
    private const int MaximumSpace = 10;

    [SerializeField] private Space _spacePrefab;

    private IGrid _grid = new SpaceGrid();
    private HashSet<Space> _allSpaces = new HashSet<Space>();


    [SerializeField] private float _xSpacing = 1f;
    [SerializeField] private float _zSpacing = 1f;
    [SerializeField] private Vector3 _offset;


    public void Initialize(LevelData levelData, EnvironmentController environmentController)
    {
        int totalSpaces = levelData.AvailableStartingSpaces;
        (_grid as SpaceGrid).SetSpacing(_xSpacing, _zSpacing);
        (_grid as SpaceGrid).SetOffset(_offset);
        _grid.CreateColumns(totalSpaces, _xSpacing);

        _allSpaces.Clear();

        Color spaceColor = environmentController.GetMatData(levelData.levelType).SpaceColor;

        for (int i = 0; i < totalSpaces; i++)
        {
            Space space = Instantiate(_spacePrefab, transform);
            space.SetIconColor(spaceColor);
            _allSpaces.Add(space);
            _grid.AddItemInColumn(space.gameObject, i);
        }

        _isFailed = false;
        _isHammerEnable = false;

        SignalBus.Subscribe<OnShooterEmptySignal>(OnShooterEmpty);
        SignalBus.Subscribe<OnSpaceOcuupy>(OnSpaceOcuupy);
        SignalBus.Subscribe<OnEvaluateSpacesSignal>(OnEvaluateSpaces);
        SignalBus.Subscribe<OnRevivalSignal>(PopSpaceFailScreen);
        SignalBus.Subscribe<OnSpaceShooterCorrectionSignal>(OnSpaceShooterCorrectionSignal);
        SignalBus.Subscribe<OnHammerEnableSignal>(OnHammerPowerUp);
        SignalBus.Subscribe<OnSpacesFullSignal>(CheckSlots);

    }

    [Button]
    public void AddSpace(bool isHidden = false)
    {
        if (_allSpaces.Count >= MaximumSpace)
        {
            Debug.LogError("Max Space Allocated");
            return;
        }

        Space newSpace = Instantiate(_spacePrefab, transform);
        if (isHidden) newSpace.SetIsHidden(true);
        _allSpaces.Add(newSpace);
        _grid.AddItemInColumn(newSpace.gameObject, _allSpaces.Count - 1);
    }

    public bool isSpaceAvailable(out Space space)
    {
        foreach (var item in _allSpaces)
        {
            if (!item.IsOccupy && !item.IsHidden)
            {
                space = item;
                return true;
            }
        }
        space = null;
        return false;
    }

    public void Tick()
    {
        if (_isHammerEnable) return;
        foreach (var tickable in _allSpaces)
        {
            tickable.Tick();
        }
    }

    private void OnShooterEmpty(OnShooterEmptySignal signal)
    {
        foreach (var space in _allSpaces)
        {
            if (space.IsShooterMatches(signal.shootable))
            {
                space.EmptySpace();
                break;
            }
        }
    }

    private void OnSpaceOcuupy(ISignal signal)
    {
        var groups = _allSpaces
          .Where(s => s.Shootable != null)
          .Select(s => (Space: s, Cannon: s.Shootable as Cannon))
          .Where(x => x.Cannon != null)
        //   .Where(y => y.Space.IsHidden == false)
        .Where(x =>
        {
            var link = x.Cannon as ILink<Cannon>;
            return link == null || !link.IsLinked;
        })
          .GroupBy(x => (x.Cannon._colorType, x.Cannon._cannonType));

        foreach (var group in groups)
        {
            if (group.Count() >= 3)
            {
                MergeCannonGroup(group.ToList());
                break;
            }
        }

        //EvaluateLevelFail();
    }

    private void MergeCannonGroup(List<(Space Space, Cannon Cannon)> cannons)
    {
        Vector3 midPos = Vector3.zero;
        foreach (var c in cannons)
            midPos += c.Space.transform.position;
        midPos /= cannons.Count;

        var centerEntry = cannons
            .OrderBy(c => Vector3.Distance(c.Space.transform.position, midPos))
            .First();

        Space centerSpace = centerEntry.Space;
        Cannon centerCannon = centerEntry.Cannon;

        centerSpace.SetOccupance(null);

        foreach (var c in cannons)
        {
            if (c.Space == centerSpace) continue;
            c.Space.EmptySpace();
        }

        int _updatedProjectile = 0;

        DOVirtual.DelayedCall(0.4f, () =>
        {
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = default, Amount = 1 });
            foreach (var c in cannons)
            {
                c.Cannon.transform.DOMoveY(c.Cannon.transform.position.y + 1f, 0.15f).SetEase(Ease.OutQuad);
                _updatedProjectile += c.Cannon.TotalProjectileRemaining;
            }

            DOVirtual.DelayedCall(0.175f, () =>
            {
                foreach (var c in cannons)
                {
                    if (c.Space == centerSpace) continue;

                    c.Cannon.transform.DOMove(centerCannon.transform.position, 0.25f)
                        .SetEase(Ease.OutCirc)
                        .OnComplete(() =>
                        {
                            centerCannon.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack);
                            Destroy(c.Cannon.gameObject);
                        });
                    DOVirtual.DelayedCall(0.2f, () =>
                    {
                        GlobalService.ParticleService.PlayParticle(ParticleType.MergeEffect, centerCannon.transform.position + (Vector3.up * 1.1f) + (new Vector3(0, 0, 0.5f)));
                        IColorLerpable colorLerpable = centerCannon as IColorLerpable;
                        colorLerpable.LerpColor(Color.black, Color.white, () => colorLerpable.LerpColor(Color.white, Color.black));
                        HapticController.Vibrate(HapticType.Hit);
                        AudioController.PlaySFX(AudioType.CannonMerge);
                        HapticController.Vibrate(HapticType.Merge);
                    });
                }
            });

            DOVirtual.DelayedCall(0.5f, () =>
            {
                centerCannon.UpdateProjectile(_updatedProjectile);
                centerCannon.transform.DOScale(1f, 0.15f);
                centerCannon.transform.DOMoveY(centerSpace.transform.position.y, 0.16f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        centerSpace.SetOccupance(centerCannon, false);
                        centerCannon.transform.localScale = Vector3.one;
                    });
            });
        });
    }

    public bool FindAvailableAdjacentPair(out Space spaceA, out Space spaceB)
    {
    CheckAdjecent:
        int freeCount = 0;
        foreach (var space in _allSpaces)
        {
            if (space.IsOccupy) continue;
            if (space.IsHidden) continue;
            freeCount++;
            foreach (var adj in GetAdjacentSpaces(space))
            {
                if (!adj.IsOccupy && !adj.IsHidden)
                {
                    spaceA = space;
                    spaceB = adj;
                    return true;
                }
            }
        }

        if (freeCount >= 2)
        {
            CompressShootersLeft();
            goto CheckAdjecent;
        }

        spaceA = null;
        spaceB = null;
        return false;
    }

    private List<Space> GetAdjacentSpaces(Space current)
    {
        List<Space> adjacent = new List<Space>();

        foreach (var space in _allSpaces)
        {
            if (space == current) continue;

            if (space.RowIndex == current.RowIndex &&
                Mathf.Abs(space.ColumnIndex - current.ColumnIndex) == 1)
            {
                adjacent.Add(space);
            }

            if (space.ColumnIndex == current.ColumnIndex &&
                Mathf.Abs(space.RowIndex - current.RowIndex) == 1)
            {
                adjacent.Add(space);
            }
        }

        return adjacent;
    }

    private void CompressShootersLeft()
    {
        List<Space> orderedSpaces = _allSpaces
            .OrderBy(s => s.ColumnIndex)
            .ToList();

        List<IShootable> shooters = new List<IShootable>();
        foreach (var space in orderedSpaces)
        {
            if (space.IsOccupy && space.Shootable != null && !space.IsHidden)
            {
                shooters.Add(space.Shootable);
                space.EmptySpace();
            }
        }

        if (shooters.Count == 0)
            return;

        int insertIndex = 0;

        for (int i = 0; i < shooters.Count; i++)
        {
            while (insertIndex < orderedSpaces.Count &&
                   (orderedSpaces[insertIndex].IsHidden || orderedSpaces[insertIndex].IsOccupy))
            {
                insertIndex++;
            }

            if (insertIndex >= orderedSpaces.Count)
                break;

            orderedSpaces[insertIndex].SetOccupance(shooters[i], Move: true);

            insertIndex++;
        }
    }

    private Space _popSpace;
    public Space GetRandomSpaceForPop()
    {
        var validSpaces = _allSpaces
           .Where(s => s.IsOccupy && !s.IsHidden && s.Shootable != null)
           .ToList();

        if (validSpaces.Count == 0)
        {
            return null;
        }

        Space selectedSpace = validSpaces[Random.Range(0, validSpaces.Count)];
        _popSpace = selectedSpace;
        return selectedSpace;
    }
    public void Pop_Shooter()
    {
        if (_popSpace == null) return;
        Space targetHiddenSpace = _allSpaces
            .FirstOrDefault(s => s.IsHidden && !s.IsOccupy);

        if (targetHiddenSpace == null)
        {
            AddSpace(isHidden: true);
            targetHiddenSpace = _allSpaces.Last();
        }

        Space selectedSpace = _popSpace;
        IShootable shooter = selectedSpace.Shootable;

        selectedSpace.EmptySpace();

        targetHiddenSpace.SetOccupance(shooter, Move: true);
        _popSpace = null;
    }

    private bool _isFailed = false;
    [Button]
    private void EvaluateLevelFail()
    {
        if (_isFailed) return;
        if (!SpacesAreFull()) return;
        if (AnyShooterCanShoot()) return;
        _isFailed = true;
        Debug.LogError("OUT OF SPACE");
        // SignalBus.Publish(new OnlevelFailSignal { levelFailType = LevelFailType.OutOFSpace });
    }

    public bool SpacesAreFull()
    {
        foreach (var space in _allSpaces)
        {
            if (!space.IsOccupy && !space.IsHidden) return false;
        }
        return true;
    }
    private bool AnyShooterCanShoot()
    {
        foreach (var space in _allSpaces)
        {
            if (space.IsOccupy)
            {
                if (space.Shootable.CanShoot) return true;
            }
        }
        return false;
    }

    private Tween _spaceChecker;
    private void OnEvaluateSpaces(ISignal signal)
    {
        _spaceChecker?.Kill();
        _spaceChecker = DOVirtual.DelayedCall(1, () => EvaluateLevelFail()).SetUpdate(true); ;
    }

    public void Pop_Shooter(Space popSpace)
    {
        if (popSpace == null) return;
        if (!popSpace.IsOccupy || popSpace.Shootable == null) return;

        Space targetHiddenSpace = _allSpaces
            .FirstOrDefault(s => s.IsHidden && !s.IsOccupy);

        if (targetHiddenSpace == null)
        {
            AddSpace(isHidden: true);
            targetHiddenSpace = _allSpaces.Last();
        }

        IShootable shooter = popSpace.Shootable;

        popSpace.EmptySpace();
        targetHiddenSpace.SetOccupance(shooter, Move: true);
    }
    Space spaceToPop;

    private void PopSpaceFailScreen(OnRevivalSignal signal)
    {
        Debug.LogError("POP SPACE CALLED");

        if (AllSpacesFull(10))
        {
            Debug.LogError("COMING INSIDE THIS CHECK");
            return;
        }
        StartCoroutine(Pop());
    }

    private IEnumerator Pop()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(0.75f);
            spaceToPop = GetRandomSpaceForPop();
            Pop_Shooter(spaceToPop);
            _isFailed = false;
        }
    }

    public bool AnySpaceOccupied()
    {
        foreach (var s in _allSpaces)
        {
            if (s.IsOccupy) return true;
        }
        return false;
    }
    int _spaceCount = 0;
    public bool AllSpacesFull(int n = 5)
    {
        _spaceCount = 0;
        foreach (var s in _allSpaces)
        {
            if (s.IsOccupy && !s.IsHidden)
            {
                _spaceCount++;
                if (_spaceCount == n)
                {
                    _spaceCount = 0;
                    return true;
                }
            }
        }
        return false;
    }

    private void OnSpaceShooterCorrectionSignal(OnSpaceShooterCorrectionSignal signal)
    {
        int remainingToReduce = signal.AmountToReduce;

        foreach (var space in _allSpaces)
        {
            if (remainingToReduce <= 0)
                break;

            if (!space.IsOccupy)
                continue;

            var cannon = space.Shootable as Cannon;
            if (cannon == null)
                continue;

            if (cannon._colorType != signal.colorType)
                continue;

            int cannonAmmo = cannon.TotalProjectileRemaining;

            int reduceFromThis = Mathf.Min(cannonAmmo, remainingToReduce);

            int newAmount = cannonAmmo - reduceFromThis;
            cannon.UpdateProjectile(newAmount);

            remainingToReduce -= reduceFromThis;

            if (newAmount <= 0)
            {
                space.EmptySpace();
                Destroy(cannon.gameObject);
            }
        }
    }

    private bool _isHammerEnable = false;
    private void OnHammerPowerUp(OnHammerEnableSignal signal)
    {
        _isHammerEnable = signal.IsEnable;
    }

    private int _count;
    private void CheckSlots(OnSpacesFullSignal signal)
    {
        _count = 0;

        foreach (var space in _allSpaces)
        {
            if (space.IsOccupy)
            {
                _count++;
            }
        }

        if (_count >= 9)
        {
            //Debug.LogError(_count);
            SignalBus.Publish(new SlotsFullSignal { isSlotsFull = true });
        }
        else
        {
            SignalBus.Publish(new SlotsFullSignal { isSlotsFull = false });
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnShooterEmptySignal>(OnShooterEmpty);
        SignalBus.Unsubscribe<OnSpaceOcuupy>(OnSpaceOcuupy);
        SignalBus.Unsubscribe<OnEvaluateSpacesSignal>(OnEvaluateSpaces);
        SignalBus.Unsubscribe<OnRevivalSignal>(PopSpaceFailScreen);
        SignalBus.Unsubscribe<OnSpaceShooterCorrectionSignal>(OnSpaceShooterCorrectionSignal);
        SignalBus.Unsubscribe<OnHammerEnableSignal>(OnHammerPowerUp);
        SignalBus.Unsubscribe<OnSpacesFullSignal>(CheckSlots);
    }
}
public class OnEvaluateSpacesSignal : ISignal { }
public class OnRevivalSignal : ISignal { }

public class OnSpacesFullSignal : ISignal { }
public class SlotsFullSignal : ISignal
{
    public bool isSlotsFull;
}