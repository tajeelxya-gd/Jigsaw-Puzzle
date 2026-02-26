using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[RequireComponent(typeof(CannonGridSystem))]
public class CannonController : MonoBehaviour
{
    [Header("References")]
    private LevelData _levelData;
    [SerializeField] private CannonData _cannonData;

    [Header("Settings")]
    [SerializeField][Range(1, 10)] private float _columnDistance = 2f;

    private IGrid _grid;
    private IFactory<Cannon, ShooterType> _factory;

    private IEnemyProvider _enemyProvider;
    private bool _isPoolCreated = false;
    private SpaceController _spaceController;


    public void Initialize(LevelData levelData, SpaceController spaceController)
    {
        _spaceController = spaceController;
        _levelData = levelData;

        if (_enemyProvider == null)
            _enemyProvider = FindAnyObjectByType<EnemyGridSystem>(FindObjectsInactive.Exclude);
        if (_factory == null)
            _factory = new CannonFactory(_cannonData);

        if (_grid == null)
            _grid = GetComponent<IGrid>();
        _grid.CreateColumns(_levelData.cannonData.Length, _columnDistance);

        if (!_isPoolCreated)
        {
            PoolManager.CreatePool(_cannonData.cannons[0].projectile, 5, 20);
            PoolManager.GetPool<Projectile>().Prewarm(6);
            _isPoolCreated = true;
        }

        LoadFromLevel();
    }
    private void LoadFromLevel()
    {
        var gridItems = new Cannon[_levelData.cannonData.Length][];

        for (int i = 0; i < _levelData.cannonData.Length; i++)
        {
            gridItems[i] = new Cannon[_levelData.cannonData[i].cannonColumns.Length];
            for (int J = 0; J < _levelData.cannonData[i].cannonColumns.Length; J++)
            {
                // if (_levelData.cannonData[i].cannonColumns[J].isHidden) continue;
                Cannon cannon = _factory.Create(_levelData.cannonData[i].cannonColumns[J].shooterType, null);
                cannon.Initialize(_levelData.cannonData[i].cannonColumns[J].colorType, _levelData.cannonData[i].cannonColumns[J].ProjectileAmount, _enemyProvider, _levelData.cannonData[i].cannonColumns[J].shooterType,
                _levelData.cannonData[i].cannonColumns[J].isHidden);
                _grid.AddItemInColumn(cannon, i);
                gridItems[i][J] = cannon;
            }
        }

        for (int i = 0; i < _levelData.cannonData.Length; i++)
        {
            for (int j = 0; j < _levelData.cannonData[i].cannonColumns.Length; j++)
            {
                var columnData = _levelData.cannonData[i].cannonColumns[j];
                if (columnData.isConnected)
                {
                    Cannon source = gridItems[i][j];
                    Cannon target = gridItems[columnData.connectedColumn][columnData.connectedRow];

                    if (source is ILink<Cannon> linkSource)
                    {
                        if (!linkSource.IsLinked)
                            linkSource.Link(target, true);
                    }
                }
            }
        }

        // if (!GlobalService.GameData.Data.OnBoardPowerUpType.Contains(PowerupType.SlotPopper) && GlobalService.GameData.Data.LevelIndex == 25)
        //     AutoMateCannonMovementForSlotPopperOnBoard();
    }

    private void AutoMateCannonMovementForSlotPopperOnBoard()
    {
        if (_grid is CannonGridSystem cannonGrid)
        {
            List<IShootable> shootables = cannonGrid.GetShootablesForSlotPopper();
            foreach (var shooter in shootables)
            {
                if (_spaceController.isSpaceAvailable(out Space space))
                {
                    space.SetOccupance(shooter);
                }
            }
            SignalBus.Publish(new OnShooterRemoveFromGridSignal { shootable = shootables });
        }
    }

    public IShootable GetInterectableForMagnetOnBoard()
    {
        IShootable interectable = null;
        if (_grid is CannonGridSystem cannonGrid)
        {
            interectable = cannonGrid.GetInterectableShooterForMagnetOnBoard();
        }
        return interectable;
    }

    public void Shuffle()
    {
        if (_grid is CannonGridSystem cannonGrid)
            cannonGrid.Shuffle();
    }

    private CannonGridSystem _cannonGrid;
    public bool HasCannon()
    {
        if (!_cannonGrid)
            _cannonGrid = _grid as CannonGridSystem;
        return _cannonGrid.HasAnyCannon();
    }
}