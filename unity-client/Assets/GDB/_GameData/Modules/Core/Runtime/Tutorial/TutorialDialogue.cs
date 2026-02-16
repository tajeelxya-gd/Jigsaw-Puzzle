using System;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TutorialDialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dialoguetext;
    [SerializeField] private GameObject _descriptionText;
    [SerializeField] private Button _gotItButton;
    [SerializeField] private InfoBoxUI[] _infoBoxUI;
    private void Start()
    {
        _gotItButton.onClick.AddListener(OnClickGotItButton);
    }
    private Tween _btnScaleTween;
    public void ShowDialogueBox(string dialogueContent, bool showButton = false, TutorialManager.InfoBoxDataBlock infoBoxDataBlock = null)
    {
        AnimateDialogueBox();
        _dialoguetext.text = dialogueContent;
        _gotItButton.gameObject.SetActive(showButton);
        _descriptionText.gameObject.SetActive(!showButton);

        if (infoBoxDataBlock != null)
            foreach (var infoBoxUI in _infoBoxUI)
            {
                if (infoBoxUI.InfoBox == infoBoxDataBlock.InfoBoxType)
                {
                    infoBoxUI.BoxObject.gameObject.SetActive(true);
                    infoBoxUI.ValueTxt.text = infoBoxDataBlock.Value.ToString();
                }
            }
        if (showButton)
        {
            _btnScaleTween?.Kill();
            _btnScaleTween = _gotItButton.transform.DOScale(1.1f, 1).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void AnimateDialogueBox()
    {
        gameObject.SetActive(true);
        transform.DOScale(1, 0.5f).From(0.5f).SetEase(Ease.OutBack);
    }

    void OnClickGotItButton()
    {
        SignalBus.Publish(new OnTutorialActivated { IsActivated = false });
        AudioController.PlaySFX(AudioType.PanelClose);
        
    }

    public void HideDialogueBox()
    {
        gameObject.SetActive(false);

    }

    private void OnDisable()
    {
        _btnScaleTween?.Kill();
        foreach (var infoBoxUI in _infoBoxUI)
        {
            infoBoxUI.BoxObject.gameObject.SetActive(false);
        }
    }


    [System.Serializable]
    class InfoBoxUI
    {
        public TutorialManager.InfoBoxType InfoBox;
        public GameObject BoxObject;
        public TextMeshProUGUI ValueTxt;
    }
}
