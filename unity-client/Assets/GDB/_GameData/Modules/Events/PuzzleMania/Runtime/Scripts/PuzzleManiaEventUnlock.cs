using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManiaEventUnlock : MonoBehaviour
{
    [SerializeField] private GameObject _locked;
    [SerializeField] private GameObject _unlocked;

    public bool _hasEventBeenUnlocked = false;
    private PuzzleManiaEventData _puzzleManiaEventData;
    private DataBaseService<PuzzleManiaEventData> _puzzleManiaEventDataService;
    [Title("OnBoarding")]
    [SerializeField] private OnBoardingConfig.OnBoardingType _onBoardingType;
    GameData _gameData;

    private void Start()
    {
        _gameData = GlobalService.GameData;
        _puzzleManiaEventDataService = new DataBaseService<PuzzleManiaEventData>();
        _puzzleManiaEventData = _puzzleManiaEventDataService.Load_Get();
        if (_puzzleManiaEventData == null)
        {
            _puzzleManiaEventData = new PuzzleManiaEventData();
            _puzzleManiaEventData.isPuzzleManiaEventUnlocked = false;
            _puzzleManiaEventDataService.Save(_puzzleManiaEventData);
        }
        SignalBus.Subscribe<PuzzleManiaEventUnlockSignal>(UnlockPuzzleManiaEvent);
        Test();
        _hasEventBeenUnlocked = _puzzleManiaEventData.isPuzzleManiaEventUnlocked;
        UpdateUI();
        DOVirtual.DelayedCall(Time.deltaTime * 10, LookForOnBoardingPanel);
    }
    void UpdateUI()
    {
        _locked.gameObject.SetActive(_gameData.Data.LevelIndex < (int)_onBoardingType);
        _unlocked.gameObject.SetActive(_gameData.Data.LevelIndex >= (int)_onBoardingType);
    }

    void LookForOnBoardingPanel()
    {
        if (_unlocked.activeInHierarchy) SendMessage("InitOnBoarding");
    }
    private void UnlockPuzzleManiaEvent(PuzzleManiaEventUnlockSignal signal)
    {
        UpdateUI();
    }


    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PuzzleManiaEventUnlockSignal>(UnlockPuzzleManiaEvent);
    }

    [Button]
    public void Test()
    {
        SignalBus.Publish(new PuzzleManiaEventUnlockSignal());
    }
}

public class PuzzleManiaEventUnlockSignal : ISignal
{
}
public class PuzzleManiaEventData
{
    public bool isPuzzleManiaEventUnlocked;
}