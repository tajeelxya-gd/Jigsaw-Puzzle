using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private ParticleData _particleData;
    [SerializeField] private EnvironmentController _environmentController;
    [SerializeField] private TutorialManager _tutorialManager;
    [SerializeField] private PlayerRaycastController _playerController;
    [SerializeField] private CannonController _cannonController;
    [SerializeField] private SpaceController _spaceController;
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private LevelController _levelController;
    [SerializeField] private SettingsManager _settingsManager;
    [SerializeField] private PoweupController _poweupController;
    [SerializeField] private UIController _uiController;
    [SerializeField] private Wall _wall;

    private IParticleService _particleService;
    private GameSoundController _gameSoundController;

    private HashSet<ITickable> _tickables = new HashSet<ITickable>();
    private void Awake()
    {
        SignalBus.Reset();
        // DOTween.useSafeMode = false;

        ColorProvider.Initialize();
        _particleService = new ParticleService(_particleData);
        GlobalService.Initialize(_particleService);
        DOVirtual.DelayedCall(0.25f, DelayedInitialization);
        SignalBus.Subscribe<ONGameResetSignal>(_Reset);
    }

    private void DelayedInitialization()
    {
        _levelController.Initialize();
        _tutorialManager.Initialize(_levelController);
        _spaceController.Initialize(_levelController.levelData, _environmentController);
        _cannonController.Initialize(_levelController.levelData, _spaceController);
        _playerController.Initilize(_spaceController, _cannonController);
        _uiController.Initialize(_levelController.levelData, _cannonController, _spaceController);
        _enemyController.Initialize(_levelController.levelData, _tutorialManager, _uiController.LevelProgressVisual);
        _environmentController.Initialize(_levelController.levelData.levelType);
        _poweupController.Initialize(_cannonController, _enemyController, _spaceController);
        _settingsManager.Inject(_levelController.levelData);
        _wall.Initialize();
        _gameSoundController = new GameSoundController(_levelController.levelData);
        _gameSoundController.PlayBG();

        _tickables = new HashSet<ITickable>
        {
            _playerController,
            _spaceController,
            _enemyController
        };
    }

    private void _Reset(ISignal signal)
    {
        DOVirtual.DelayedCall(0.3f, () => DelayedInitialization());
    }

    private void Update()
    {
        Application.targetFrameRate = 120;
        foreach (var tickable in _tickables)
        {
            tickable.Tick();
        }
    }

    private void OnDisable()
    {
        PoolManager.ResetAllPools();
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        _environmentController.Initialize(LevelType.Easy);
#endif
        SignalBus.Unsubscribe<ONGameResetSignal>(_Reset);
    }
}
public class ONGameResetSignal : ISignal { }