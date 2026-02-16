using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
public class TutorialManager : MonoBehaviour, ITutorial
{


    [ReadOnly, ShowInInspector] private int _currentTutorialLevelIndex = 0;
    [SerializeField] private TutorialUIManager _tutorialUIManager;
    [SerializeField] TutorialDataBlock[] _tutorialDataBlock;
    [ReadOnly, SerializeField] TutorialDataBlock _currentTutorialDataBlock;
    public static bool IsTutorialActivated => _tutorialActivated;

    private static bool _tutorialActivated = false;

    
    private void Start()
    {
        SignalBus.Subscribe<OnCannonMovedToSpace>(OnCannonMoved);
        SignalBus.Subscribe<OnTutorialActivated>(OnTutorialActivatedSignal);
    }

    public void Initialize(LevelController levelController)
    {
        OnLevelLoadedSuccessfully(levelController.GetCurrentLevel());
    }

    void OnLevelLoadedSuccessfully(int levelNo)
    {
        LoadTutorialIfAvailable(levelNo);
    }

    private int _currentActions = 0;
    public void OnCannonMoved(OnCannonMovedToSpace signal)
    {
        _currentActions++;
        if (_currentActions >= _currentTutorialDataBlock._actionsCounts)
        {
            SignalBus.Publish(new OnTutorialActivated { IsActivated = false });
        }
    }

    void LoadTutorialIfAvailable(int levelNo = 0)
    {
        foreach (var tutorialBlock in _tutorialDataBlock)
        {
            ++_currentTutorialLevelIndex;
            if (tutorialBlock.DependentLevel == levelNo)
            {
                if (tutorialBlock.IsTutorialCompleted()) continue;
                _currentTutorialDataBlock = tutorialBlock;
                ActivateTutorial();
                return;
            }
        }
    }

    public bool HasFastInitialization()
    {
        return _currentTutorialDataBlock != null && _currentTutorialDataBlock.DoFastInitialization && !_currentTutorialDataBlock.IsTutorialCompleted();
    }

    void ResetState()
    {
        _currentActions = 0;
    }

    void ActivateTutorial()
    {
        ResetState();
        _currentTutorialDataBlock.ActivateTutorial();
        if (_currentTutorialDataBlock.ShowMask)
            _tutorialUIManager.ShowUIMask(_currentTutorialDataBlock.SoftMaskFocusRect, _currentTutorialDataBlock.HandAnimationRect, _currentTutorialDataBlock._actionsCounts != 0);
        SignalBus.Publish(new OnTutorialActivated { IsActivated = true, OverrideInput = _currentTutorialDataBlock.OverrideInput });
        if (_currentTutorialDataBlock.ShowDialogueBox)
        {
            _currentTutorialDataBlock.InfoBoxType = !_currentTutorialDataBlock.HasInfoBox ? InfoBoxType.None : _currentTutorialDataBlock.InfoBoxType;
            SignalBus.Publish(new OnShowTutorialDialogue
            {
                DialogueTxt = _currentTutorialDataBlock.DialogueText,
                IsButtonActivated = _currentTutorialDataBlock._actionsCounts == 0,
                OverrideTransform = _currentTutorialDataBlock.DialogueRectTransform,
                InfoBox = new InfoBoxDataBlock { InfoBoxType = _currentTutorialDataBlock.InfoBoxType, Value = _currentTutorialDataBlock.Value }
            });
        }
    }

    void OnTutorialActivatedSignal(OnTutorialActivated signal)
    {
        _tutorialActivated = signal.IsActivated;
        if (_tutorialActivated == false)
            OnTutorialCompleted();

    }

    void OnTutorialCompleted()
    {
        ++_currentTutorialLevelIndex;
        _currentTutorialDataBlock.SetTutorialCompleted();
    }



    private void OnDestroy()
    {
        //  SignalBus.Unsubscribe<OnLevelLoadedSignal>(OnLevelLoadedSuccessfully);
        SignalBus.Unsubscribe<OnCannonMovedToSpace>(OnCannonMoved);
        SignalBus.Unsubscribe<OnTutorialActivated>(OnTutorialActivatedSignal);


    }
    public enum InfoBoxType
    {
        None, Piggy
    }
    public class InfoBoxDataBlock
    {
        public TutorialManager.InfoBoxType InfoBoxType;
        public int Value;
    }
}

public interface ITutorial
{
    public bool HasFastInitialization();
}

[System.Serializable]
class TutorialDataBlock
{
    [Space]
    [Space]
    [Title("- Tutorial Step -")]
    public int DependentLevel = 0;
    public int _actionsCounts = 0;
    public string TutorialStep_Name = "";
    public bool ShowDialogueBox = false;
    public bool ShowMask = true;
    public bool OverrideInput = true;
    [ShowIf("@ShowDialogueBox == true")] public bool HasInfoBox = false;
    [ShowIf("@HasInfoBox == true")] public TutorialManager.InfoBoxType InfoBoxType;
    [ShowIf("@HasInfoBox == true")] public int Value;
    [ShowIf("@ShowDialogueBox == true")] public string DialogueText;
    [ShowIf("@ShowDialogueBox == true")] public RectTransform DialogueRectTransform;

    public bool DoFastInitialization = false;
    public RectTransform SoftMaskFocusRect;
    public RectTransform HandAnimationRect;
    public UnityEvent OnTutorialStart_Event;
    public UnityEvent OnTutorialComplete_Event;
    public List<GameObject> Objects_ToActivate;
    public List<GameObject> Objects_ToDeActivatea;
    private const string TUTORIAL_KEY = "TutorialManager";

    public void SetTutorialCompleted()
    {
        PlayerPrefs.SetInt(TUTORIAL_KEY + DependentLevel.ToString(), 1);
    }

    public bool IsTutorialCompleted()
    {
        return PlayerPrefs.GetInt(TUTORIAL_KEY + DependentLevel.ToString()) == 1;
    }

    public void ActivateTutorial()
    {
        OnTutorialStart_Event?.Invoke();

        foreach (var obj in Objects_ToActivate)
            if (obj) obj.SetActive(true);

        foreach (var obj in Objects_ToDeActivatea)
            if (obj) obj.SetActive(false);
    }


}

