using System;
using UnityEngine;

public class TutorialUIManager : MonoBehaviour
{
    [SerializeField] SoftMaskUI _softMaskUI;
    [SerializeField] TutorialDialogue _tutorialDialogue;
    [SerializeField] GameObject _tutorialUI, _trackPad;


    private RectTransform _defaultDialogueBoxAnchoredTransorm;
    private void Start()
    {
        SignalBus.Subscribe<OnTutorialActivated>(OnTutorialActivated);
        SignalBus.Subscribe<OnShowTutorialDialogue>(ShowTutorialDialogueBox);
        _defaultDialogueBoxAnchoredTransorm = _tutorialDialogue.GetComponent<RectTransform>();
    }

    void OnTutorialActivated(OnTutorialActivated signal)
    {
        _tutorialUI.gameObject.SetActive(signal.IsActivated);
        if (!signal.IsActivated)
            _tutorialDialogue.gameObject.SetActive(false);
        _trackPad.SetActive(signal.IsActivated && signal.OverrideInput);
    }

    public void ShowUIMask(RectTransform target, RectTransform handRect, bool ShowHand)
    {
        _softMaskUI.ShowFocusesMask(target, handRect, ShowHand);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnTutorialActivated>(OnTutorialActivated);
    }

    void ShowTutorialDialogueBox(OnShowTutorialDialogue signal)
    {
        AudioController.PlaySFX(AudioType.PanelPop);
        _tutorialDialogue.ShowDialogueBox(signal.DialogueTxt, signal.IsButtonActivated, signal.InfoBox);
        if (signal.OverrideTransform != null)
            _tutorialDialogue.GetComponent<RectTransform>().anchoredPosition = signal.OverrideTransform != null
                ? signal.OverrideTransform.anchoredPosition
                : _defaultDialogueBoxAnchoredTransorm.anchoredPosition;

        _tutorialDialogue.GetComponent<RectTransform>().sizeDelta = signal.OverrideTransform != null
            ? signal.OverrideTransform.sizeDelta
            : _defaultDialogueBoxAnchoredTransorm.sizeDelta;
    }
}
