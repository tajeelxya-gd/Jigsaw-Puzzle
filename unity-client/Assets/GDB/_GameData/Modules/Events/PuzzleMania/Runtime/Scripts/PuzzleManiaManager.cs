using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PuzzleManiaManager : MonoBehaviour
{
    [SerializeField] private PuzzleManiaUI _puzzleManiaUI;
    [SerializeField] private PuzzleManiaPuzzle _puzzle;
    [SerializeField] private PuzzleManiaReward _puzzleManiaReward;
    [SerializeField] private PuzzleManiaBar _puzzleManiaBar;
    [SerializeField] private PuzzleManiaUIData _maniaUIData;
    [SerializeField] private ScrollRectArraySnapper _scroll;
    [SerializeField] private HolderManager[] _holder;

    [SerializeField] private Sprite _normal, _green, _gold;
    [SerializeField] private Transform _startPoint;
    private DataBaseService<PuzzleManiaSaveData> _saveDatabase;
    private PuzzleManiaSaveData _saveData;

    private int _currentMilestoneIndex = 0;
    private int _totalBlocksUsed = 0;
    private int _currentPreset = 0;
    private UIData[] _milestones;
    [SerializeField] private GameObject _puzzleManiaRoot;
    [SerializeField] private Button _puzzleManiaButton;
    [SerializeField] private GameObject _puzzleCompletePanel;
    [SerializeField] private Image _puzzleImage;
    [SerializeField] private Sprite[] _fullPuzzles;

    [Title("OnBoarding")] [SerializeField] private OnBoardingConfig.OnBoardingType _onBoardingType;

    GameData _gameData;
    private int _currentPuzzleIndex = 0;

    [SerializeField] private HorizontalScrollSnapper _scrollSnap;
    [SerializeField] private float _targetMultiplier = 1.5f;

    private void Awake()
    {
        _saveDatabase = new DataBaseService<PuzzleManiaSaveData>();
        _saveData = _saveDatabase.Load_Get();
        _currentMilestoneIndex = _saveData._currentMilestoneIndex;
        _currentPuzzleIndex = _saveData._puzzleIndex;
        _totalBlocksUsed = _saveData._totalBlocksUsed;
        Debug.LogError("BlocksUsed: " + _totalBlocksUsed);

        _milestones = _maniaUIData._maniaUIPresets[_currentPreset]._maniaRewards;

        SignalBus.Subscribe<PuzzleManiaBlocksUsed>(OnBlocksUsed);
        SignalBus.Subscribe<PuzzleManiaReset>(ResetDay);
        SignalBus.Subscribe<PuzzleManiaPanelOpenSignal>(SnapScroll);
    }

    private void Start()
    {
        _puzzleManiaUI.SetupUI(_milestones);
        if (_currentMilestoneIndex < _holder.Length)
        {
            _holder[_currentMilestoneIndex].SetHolderImage(_green);
        }

        int max = Mathf.Min(_currentMilestoneIndex, _milestones.Length);
        for (int i = 0; i < max; i++)
        {
            _puzzleManiaUI.UpdateUI(i);
        }

        int piecesForCurrentPuzzle = _puzzle.GetPieceCountUnlocked(_currentPuzzleIndex);
        for (int i = 0; i < piecesForCurrentPuzzle; i++)
        {
            _puzzle.UnlockPiece(_currentPuzzleIndex, i + 1);
        }



        for (int x = 0; x < 3; x++)
        {
            if (_puzzle.IsPuzzleCompleted(x))
            {
                Debug.LogError("SHOWING COMPLETED PUZZLE: " + x);
                _puzzle.ShowFullPuzzle(x);
            }
        }

        PuzzleManiaUIPreset currentPresetData = _maniaUIData._maniaUIPresets[_currentPreset];
        if (currentPresetData._maniaRewards.Length > 0)
        {
            UIData lastReward = currentPresetData._maniaRewards[^1];
            _puzzleManiaUI.SetGrandPrize(lastReward._RewardProgress.rewardChestAmount);
        }
        StartCoroutine(ScrollToCurrentMilestone());
        UpdateBarAtStart();
        _gameData = GlobalService.GameData;
        int rewardAmount = GlobalService.GameData.Data.CurrentLevelEnemies;
        UpdateUI();

        if (_gameData.Data.LevelNumber < (int)_onBoardingType)
        {
            _gameData.Data.CurrentLevelEnemies = 0;
            _gameData.Save();
            return;
        }

        if (rewardAmount != 0)
        {
            PopCommandExecutionResponder.AddCommand(new PuzzleRushShowCommand(
                PopCommandExecutionResponder.PopupPriority.Low,
                execution => { StartCoroutine(WaitBeforeGift(rewardAmount)); }));
        }

        DOVirtual.DelayedCall(Time.deltaTime * 10, LookForOnBoardingPanel);
    }
    

    private IEnumerator WaitBeforeGift(int rewardAmount)
    {
        yield return null;
        UnityAction onCompleteAction = () =>
        {
            Debug.LogError("Puzzle Mania Value Updated!!");
            OnBlocksUsed(
                new PuzzleManiaBlocksUsed() { _blocksNumber = GlobalService.GameData.Data.CurrentLevelEnemies });
            GlobalService.GameData.Data.CurrentLevelEnemies = 0;
            GlobalService.GameData.Save();
        };
        GlobalService.IBulkPopService.PlayEffect(rewardAmount, PopBulkService.BulkPopUpServiceType.EnemyBlocks,
            _startPoint.position, 10, onCompleteAction);
    }

    void UpdateUI()
    {
        _puzzleManiaRoot.gameObject.SetActive(_gameData.Data.LevelNumber >= (int)_onBoardingType);
    }

    void LookForOnBoardingPanel()
    {
        if (_puzzleManiaButton.gameObject.activeInHierarchy)
        {
            _gameData.Data.IsPuzzleManiaUnlocked = true;
            _gameData.Save();
            SendMessage("InitOnBoarding", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void SnapScroll(PuzzleManiaPanelOpenSignal signal)
    {
        StartCoroutine(ScrollToCurrentMilestone());
    }

    private IEnumerator ScrollToCurrentMilestone()
    {
        yield return new WaitForEndOfFrame();
        //Debug.LogError("ADSKLJHJKLASDHFKLJHSAD");
        _scroll.SnapToItem(_currentMilestoneIndex);
        _scrollSnap.SnapToItemInstant(_currentPuzzleIndex);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PuzzleManiaReset>(ResetDay);
        SignalBus.Unsubscribe<PuzzleManiaBlocksUsed>(OnBlocksUsed);
        SignalBus.Unsubscribe<PuzzleManiaPanelOpenSignal>(SnapScroll);
    }

    private void OnBlocksUsed(PuzzleManiaBlocksUsed signal)
    {
        _totalBlocksUsed += signal._blocksNumber;
        _saveData._totalBlocksUsed = _totalBlocksUsed;
        _saveDatabase.Save(_saveData);
        StartCoroutine(CheckMilestoneCoroutine());
    }

    private void CheckPuzzleProgression()
    {
        // If current puzzle completed -> move to next
        if (_puzzle.IsPuzzleCompleted(_currentPuzzleIndex))
        {
            _currentPuzzleIndex++;
            _saveData._puzzleIndex = _currentPuzzleIndex;
            _saveDatabase.Save(_saveData);

            //Debug.LogError($"Puzzle {_currentPuzzleIndex - 1} completed!");
        }

        // If ALL puzzles completed -> reset everything
        if (_currentPuzzleIndex >= _puzzle.TotalPuzzleCount())
        {
            //Debug.LogError("All puzzles completed - resetting Puzzle Mania");

            _currentPuzzleIndex = 0;
            _saveData._puzzleIndex = 0;
            _saveDatabase.Save(_saveData);

            _puzzle.ResetPuzzle();
        }
    }

    private IEnumerator CheckMilestoneCoroutine()
    {
        bool hasRequestedMilestone = false;
        while (_currentMilestoneIndex < _milestones.Length &&
               _totalBlocksUsed >= _milestones[_currentMilestoneIndex]._requiredBlocks*_targetMultiplier)
        {
            UIData data = _milestones[_currentMilestoneIndex];

            _puzzleManiaUI.UpdateUI(_currentMilestoneIndex);
            _scroll.SnapToItem(_currentMilestoneIndex);

            CheckPuzzleProgression();

            _puzzleManiaBar.UpdateFillBar(1f);

            int milestoneIndex = _currentMilestoneIndex + 1;
            if (milestoneIndex % 3 == 0)
            {
                _scrollSnap.SnapToItemInstant(_currentPuzzleIndex);
                _puzzle.UnlockPiece(_currentPuzzleIndex, milestoneIndex / 3);
                SignalBus.Publish(new OpenPuzzleManiaSignal(){_playAnimation = true});
                hasRequestedMilestone = true;
                //Debug.LogError("RUNNING");
            }
           
            if (milestoneIndex >= 27)
            {
                _puzzleCompletePanel.SetActive(true);
                _puzzleImage.sprite= _fullPuzzles[_currentPuzzleIndex];
            }
            _totalBlocksUsed -= (int)(data._requiredBlocks * _targetMultiplier);
            _currentMilestoneIndex++;
            SendAnalyticEvent();
            yield return new WaitForSeconds(_puzzleManiaBar._fillDuration);
            _puzzleManiaReward.GiveReward(data._RewardProgress);
            if (_currentMilestoneIndex < _milestones.Length)
            {
                _puzzleManiaBar.UpdateRewardImage(_milestones[_currentMilestoneIndex]._RewardProgress._rewardIcon);
                _holder[_currentMilestoneIndex].SetHolderImage(_green);
            }
            
            _saveData._currentMilestoneIndex = _currentMilestoneIndex;
            _saveData._totalBlocksUsed = _totalBlocksUsed;
            _saveDatabase.Save(_saveData);
        }

        UpdateBar();
        if (!hasRequestedMilestone)
        {
            PopCommandExecutionResponder.RemoveCommand<PuzzleRushShowCommand>();
        }
    }

    void SendAnalyticEvent()
    {
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData,"Events",nameof(AnalyticEventType.PuzzleMania), "Progression", "Step",(_currentMilestoneIndex +1).ToString());
    }

    private void UpdateBar()
    {
        if (_milestones == null || _milestones.Length == 0) return;

        if (_currentMilestoneIndex >= _milestones.Length)
        {
            _puzzleManiaBar.UpdateFillBar(1f);
            if (_milestones.Length > 0)
                _puzzleManiaBar.UpdateProgressText(
                    (int)(_milestones[^1]._requiredBlocks * _targetMultiplier),
                    (int)(_milestones[^1]._requiredBlocks * _targetMultiplier)
                );
            //Debug.LogError("Milestone Achieved !!!");
            return;
        }

        int target = (int)(_milestones[_currentMilestoneIndex]._requiredBlocks * _targetMultiplier);
        int current = Mathf.Min(_totalBlocksUsed, target);
        float fill = target > 0 ? (float)current / target : 0;

        _saveData._targetBlocks = target;
        _saveData._currentBlocks = current;
        _saveData._fillAmount = fill;
        _saveDatabase.Save(_saveData);

        _puzzleManiaBar.UpdateFillBar(fill);
        _puzzleManiaBar.UpdateProgressText(current, target);
    }

    private void UpdateBarAtStart()
    {
        if (_milestones == null || _milestones.Length == 0) return;

        if (_currentMilestoneIndex >= _milestones.Length)
        {
            _puzzleManiaBar.UpdateFillBar(1f);
            _puzzleManiaBar.UpdateProgressText(
                (int)(_milestones[^1]._requiredBlocks * _targetMultiplier),
                (int)(_milestones[^1]._requiredBlocks * _targetMultiplier)
            );            int index = Mathf.Min(_saveData._currentMilestoneIndex, _milestones.Length - 1);
            _puzzleManiaBar.UpdateRewardImage(_milestones[index]._RewardProgress._rewardIcon);
            return;
        }

        if (_saveData._targetBlocks == 0)
        {
            _saveData._targetBlocks = (int)(_milestones[_currentMilestoneIndex]._requiredBlocks * _targetMultiplier);
        }

        _puzzleManiaBar.UpdateFillBar(_saveData._fillAmount);
        _puzzleManiaBar.UpdateProgressText(_saveData._currentBlocks, _saveData._targetBlocks);
        _puzzleManiaBar.UpdateRewardImage(_milestones[_saveData._currentMilestoneIndex]._RewardProgress._rewardIcon);
    }

    private void ResetDay(PuzzleManiaReset signal)
    {
        _puzzleManiaUI.ResetUI();
        _currentMilestoneIndex = 0;
        _totalBlocksUsed = 0;
        _saveData._currentMilestoneIndex = 0;
        _saveData._totalBlocksUsed = 0;
        _saveDatabase.Save(_saveData);

        _puzzleManiaUI.SetupUI(_milestones);
        if (_holder.Length > 0)
            _holder[_currentMilestoneIndex].SetHolderImage(_green);

        UpdateBar();
        StartCoroutine(ScrollToCurrentMilestone());
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData,"Events",nameof(AnalyticEventType.PuzzleMania), "Reset");
        
    }

    [Button]
    public void Test(int blocks)
    {
        OnBlocksUsed(new PuzzleManiaBlocksUsed() { _blocksNumber = blocks });
    }
}

[Serializable]
public class PuzzleManiaBlocksUsed : ISignal
{
    public int _blocksNumber;
}

[Serializable]
public class PuzzleManiaSaveData
{
    public int _currentMilestoneIndex;
    public int _totalBlocksUsed;
    public int _targetBlocks;
    public int _currentBlocks;
    public float _fillAmount;
    public int _puzzleIndex;
}