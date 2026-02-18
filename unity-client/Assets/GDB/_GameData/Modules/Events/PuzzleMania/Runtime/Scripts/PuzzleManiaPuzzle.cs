using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class PuzzleManiaPuzzle : MonoBehaviour
{
    [Header("All Puzzles")] [SerializeField]
    private FullPuzzleData[] _puzzles;

    private DataBaseService<PuzzleCompleteSave> _puzzleSaveData;
    public PuzzleCompleteSave _puzzleData;
    [SerializeField] private GameObject invisibleLayer;
    private void Awake()
    {
        Init();
        UpdateUnlockedSprites();
    }

    private void Init()
    {
        //Debug.LogError("HEREEEEEEEEEEEEEEEJKFAHSDKFJHAJSKDHFJKLA");
        if (_puzzleData != null)
        {
            //return;
        }

        _puzzleSaveData = new DataBaseService<PuzzleCompleteSave>();
        _puzzleData = _puzzleSaveData.Load_Get();

        if (_puzzleData == null)
        {
            CreateNewSaveData();
        }
        else if (_puzzleData._puzzlesCompleted == null)
        {
            CreateNewSaveData();
        }
        else if (_puzzleData._puzzlesCompleted.Length != _puzzles.Length)
        {
            bool[] newArray = new bool[_puzzles.Length];
            for (int i = 0; i < Mathf.Min(_puzzles.Length, _puzzleData._puzzlesCompleted.Length); i++)
                newArray[i] = _puzzleData._puzzlesCompleted[i];

            _puzzleData._puzzlesCompleted = newArray;
            _puzzleSaveData.Save(_puzzleData);
        }

        if (_puzzleData._piecesUnlocked == null ||
            _puzzleData._piecesUnlocked.Length != _puzzles.Length)
        {
            int[] newPieces = new int[_puzzles.Length];

            if (_puzzleData._piecesUnlocked != null)
            {
                for (int i = 0; i < Mathf.Min(_puzzles.Length, _puzzleData._piecesUnlocked.Length); i++)
                    newPieces[i] = _puzzleData._piecesUnlocked[i];
            }

            _puzzleData._piecesUnlocked = newPieces;
            _puzzleSaveData.Save(_puzzleData);
        }

        if (_puzzleData._piecesAnimated == null || _puzzleData._piecesAnimated.Length != _puzzles.Length)
        {
            _puzzleData._piecesAnimated = new PuzzlePieceAnimationData[_puzzles.Length];
            for (int i = 0; i < _puzzles.Length; i++)
            {
                _puzzleData._piecesAnimated[i] = new PuzzlePieceAnimationData();
                if (_puzzles[i]._puzzlePieces != null)
                    _puzzleData._piecesAnimated[i]._piecesAnimated = new bool[_puzzles[i]._puzzlePieces.Length];
                else
                    _puzzleData._piecesAnimated[i]._piecesAnimated = new bool[0];
            }

            _puzzleSaveData.Save(_puzzleData);
        }

        foreach (var puzzle in _puzzles)
        {
            if (puzzle._puzzlePieces == null) continue;

            puzzle._originalPuzzle = new Sprite[puzzle._puzzlePieces.Length];
            puzzle._hasAnimated = new bool[puzzle._puzzlePieces.Length];
            puzzle._piecesUnlocked = 0;

            for (int i = 0; i < puzzle._puzzlePieces.Length; i++)
            {
                if (puzzle._puzzlePieces[i] != null)
                {
                    puzzle._originalPuzzle[i] = puzzle._puzzlePieces[i].sprite;
                }
            }
        }

        for (int x = 0; x < _puzzles.Length; x++)
        {
            if (x < _puzzleData._puzzlesCompleted.Length)
            {
                _puzzles[x]._isPuzzleComplete = _puzzleData._puzzlesCompleted[x];
            }

            if (x < _puzzleData._piecesUnlocked.Length)
            {
                _puzzles[x]._piecesUnlocked = _puzzleData._piecesUnlocked[x];
                //Debug.LogError("cOMING HERE IN ASSIGNING SPRITES");
            }
            
            if (x < _puzzleData._piecesAnimated.Length)
            {
                _puzzles[x]._hasAnimated = _puzzleData._piecesAnimated[x]._piecesAnimated;
            }
        }

        SignalBus.Subscribe<PuzzleRewardGiven>(OnPanelOpen);
        SignalBus.Subscribe<OpenPuzzleManiaSignal>(ShouldPlayAnimation);
    }

    private void CreateNewSaveData()
    {
        if (_puzzleData == null)
            _puzzleData = new PuzzleCompleteSave();

        _puzzleData._puzzlesCompleted = new bool[_puzzles.Length];
        _puzzleData._piecesUnlocked = new int[_puzzles.Length];
        _puzzleData._piecesAnimated = new PuzzlePieceAnimationData[_puzzles.Length];

        for (int i = 0; i < _puzzles.Length; i++)
        {
            _puzzleData._puzzlesCompleted[i] = false;
            _puzzleData._piecesUnlocked[i] = 0;

            _puzzleData._piecesAnimated[i] = new PuzzlePieceAnimationData();
            if (_puzzles[i]._puzzlePieces != null)
                _puzzleData._piecesAnimated[i]._piecesAnimated = new bool[_puzzles[i]._puzzlePieces.Length];
            else
                _puzzleData._piecesAnimated[i]._piecesAnimated = new bool[0];
        }

        _puzzleSaveData.Save(_puzzleData);
    }


    private bool _shouldPlayAnimation = false;

    private void ShouldPlayAnimation(OpenPuzzleManiaSignal signal)
    {
        _shouldPlayAnimation = signal._playAnimation;
    }
[SerializeField] private PuzzleHandler _puzzleHandler;
    private void OnPanelOpen(PuzzleRewardGiven signal)
    {
        if (!_shouldPlayAnimation) return;
        _puzzleHandler.OpenPanel();
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        Init(); // Ensure we are ready
        foreach (var puzzle in _puzzles)
        {
            StartCoroutine(AnimateUnlockedPieces(puzzle));
        }
    }

    private WaitForSeconds _wait = new WaitForSeconds(0.5f);
    private WaitForSeconds _wait2 = new WaitForSeconds(2.5f);
    private IEnumerator AnimateUnlockedPieces(FullPuzzleData puzzle)
    {
        yield return _wait;
        int puzzleIndex = puzzle._puzzleIndex;

        for (int i = 0; i < puzzle._piecesUnlocked; i++)
        {
            if (i >= puzzle._hasAnimated.Length || puzzle._hasAnimated[i])
            {
                puzzle._puzzlePieces[i].sprite = puzzle._unlockedPuzzle[i];
               // Debug.LogError("COMING HERE IN HAS ANIMATED IF CONDITION");
                continue;
            }

            //Debug.LogError("Coming here for puzzle Animation");
            yield return _wait;
            invisibleLayer.SetActive(true);
            SignalBus.Publish(new PuzzleManiaBreakAnimationSignal
            {
                _transform = puzzle._puzzlePieces[i].transform
            });

            puzzle._puzzlePieces[i].sprite = puzzle._unlockedPuzzle[i];
            puzzle._hasAnimated[i] = true;

            // Save animation state
            if (_puzzleData._piecesAnimated != null &&
                puzzleIndex < _puzzleData._piecesAnimated.Length &&
                i < _puzzleData._piecesAnimated[puzzleIndex]._piecesAnimated.Length)
            {
                _puzzleData._piecesAnimated[puzzleIndex]._piecesAnimated[i] = true;
                _puzzleSaveData.Save(_puzzleData);
            }

            yield return _wait2;
        }

        if (puzzle._puzzlePieces != null && puzzle._piecesUnlocked >= puzzle._puzzlePieces.Length)
        {
            puzzle._isPuzzleComplete = true;
        }
        invisibleLayer.SetActive(false);
        _shouldPlayAnimation = false;
    }


    public void UnlockPiece(int puzzleIndex, int milestoneIndex)
    {
        Init();

        FullPuzzleData puzzle = GetPuzzle(puzzleIndex);
        if (puzzle == null) return;
        if (puzzle._puzzlePieces == null || puzzle._puzzlePieces.Length == 0) return;

        int pieceIndex = milestoneIndex - 1;

        if (pieceIndex < 0 || pieceIndex >= puzzle._puzzlePieces.Length)
            return;

        if (pieceIndex + 1 > puzzle._piecesUnlocked)
        {
            puzzle._piecesUnlocked = pieceIndex + 1;

            if (_puzzleData != null && _puzzleData._piecesUnlocked != null)
            {
                if (puzzleIndex < _puzzleData._piecesUnlocked.Length)
                {
                    _puzzleData._piecesUnlocked[puzzleIndex] = puzzle._piecesUnlocked;
                    _puzzleSaveData.Save(_puzzleData);
                }
            }
        }

        if (puzzle._piecesUnlocked >= puzzle._puzzlePieces.Length)
        {
            puzzle._isPuzzleComplete = true;

            if (_puzzleData != null && _puzzleData._puzzlesCompleted != null)
            {
                if (puzzleIndex < _puzzleData._puzzlesCompleted.Length)
                {
                    _puzzleData._puzzlesCompleted[puzzleIndex] = true;
                    _puzzleSaveData.Save(_puzzleData);
                    Debug.LogError("PUZZLE COMPLETED INDEX: " + puzzleIndex);
                }
            }
        }
    }
    public void UpdateUnlockedSprites()
    {
        foreach (var puzzle in _puzzles)
        {
            if (puzzle == null || puzzle._puzzlePieces == null) continue;

            int unlockedCount = puzzle._piecesUnlocked;

            for (int i = 0; i < unlockedCount; i++)
            {
                if (i >= puzzle._puzzlePieces.Length) break;
                if (puzzle._puzzlePieces[i] == null) continue;

                puzzle._puzzlePieces[i].sprite = puzzle._unlockedPuzzle[i];

                // Mark as animated so animation never plays again
                if (puzzle._hasAnimated != null && i < puzzle._hasAnimated.Length)
                {
                    puzzle._hasAnimated[i] = true;
                }

                // Sync save data (safety, no side effects)
                int puzzleIndex = puzzle._puzzleIndex;
                if (_puzzleData != null &&
                    puzzleIndex < _puzzleData._piecesAnimated.Length &&
                    i < _puzzleData._piecesAnimated[puzzleIndex]._piecesAnimated.Length)
                {
                    _puzzleData._piecesAnimated[puzzleIndex]._piecesAnimated[i] = true;
                }
            }
        }

        if (_puzzleSaveData != null && _puzzleData != null)
        {
            _puzzleSaveData.Save(_puzzleData);
        }
    }


    public int GetPieceCountUnlocked(int puzzleIndex)
    {
        FullPuzzleData puzzle = GetPuzzle(puzzleIndex);
        if (puzzle != null) return puzzle._piecesUnlocked;
        return 0;
    }

    public void ShowFullPuzzle(int index)
    {
        Init();
        FullPuzzleData puzzle = GetPuzzle(index);
        if (puzzle == null) return;

        for (int i = 0; i < puzzle._puzzlePieces.Length; i++)
        {
            if (puzzle._puzzlePieces[i] != null)
            {
                puzzle._puzzlePieces[i].sprite = puzzle._unlockedPuzzle[i];
                puzzle._hasAnimated[i] = true;
            }
        }
    }

    public FullPuzzleData GetPuzzle(int puzzleIndex)
    {
        for (int i = 0; i < _puzzles.Length; i++)
        {
            if (_puzzles[i]._puzzleIndex == puzzleIndex)
                return _puzzles[i];
        }

        return null;
    }

    public bool IsPuzzleCompleted(int puzzleIndex)
    {
        Init();
        FullPuzzleData puzzle = GetPuzzle(puzzleIndex);
        if (puzzle == null) return false;
        return puzzle._isPuzzleComplete;
    }

    public int TotalPuzzleCount()
    {
        return _puzzles.Length;
    }

    [Button]
    public void ResetPuzzle()
    {
        Init();
        //Debug.LogError("RESET FUNCTION CALLED");

        // Reset Visuals
        for (int x = 0; x < _puzzles.Length; x++)
        {
            FullPuzzleData puzzle = GetPuzzle(x);
            if (puzzle == null) continue;

            for (int i = 0; i < puzzle._puzzlePieces.Length; i++)
            {
                if (puzzle._puzzlePieces[i] != null)
                {
                    puzzle._puzzlePieces[i].sprite = puzzle._originalPuzzle[i];
                    puzzle._hasAnimated[i] = false;
                }
            }

            puzzle._piecesUnlocked = 0;
            puzzle._isPuzzleComplete = false;
        }

        // Reset Save Data
        if (_puzzleData != null)
        {
            if (_puzzleData._puzzlesCompleted == null || _puzzleData._puzzlesCompleted.Length != _puzzles.Length)
            {
                _puzzleData._puzzlesCompleted = new bool[_puzzles.Length];
            }

            for (int i = 0; i < _puzzleData._puzzlesCompleted.Length; i++)
                _puzzleData._puzzlesCompleted[i] = false;

            _puzzleSaveData.Save(_puzzleData);
        }

        for (int i = 0; i < _puzzleData._piecesUnlocked.Length; i++)
        {
            _puzzleData._piecesUnlocked[i] = 0;
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PuzzleRewardGiven>(OnPanelOpen);
        SignalBus.Unsubscribe<OpenPuzzleManiaSignal>(ShouldPlayAnimation);
    }
}

[Serializable]
public class FullPuzzleData
{
    public int _puzzleIndex;
    public Image[] _puzzlePieces;
    public Sprite[] _unlockedPuzzle;
    public bool _isPuzzleComplete;
    [HideInInspector] public Sprite[] _originalPuzzle;
    [HideInInspector] public bool[] _hasAnimated;
    [HideInInspector] public int _piecesUnlocked;
}

public class PuzzleCompleteSave
{
    public bool[] _puzzlesCompleted;
    public int[] _piecesUnlocked;
    public PuzzlePieceAnimationData[] _piecesAnimated;
}

[Serializable]
public class PuzzlePieceAnimationData
{
    public bool[] _piecesAnimated;
}

public class PuzzleRewardGiven:ISignal
{
}