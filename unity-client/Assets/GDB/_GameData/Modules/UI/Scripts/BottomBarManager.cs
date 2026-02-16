using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BottomBarManager : MonoBehaviour
{
    [SerializeField] private Button []_bottomButtons;
   
    [SerializeField] private int _defaultButtnSelected = 1;
    private void Awake()
    {
        for (int i = 0; i < _bottomButtons.Length; i++)
        {
            var i1 = i;
            _bottomButtons[i].onClick.AddListener(()=>ButtonSelected(i1));
        }
    }

    private void Start()
    {
        ButtonSelected(_defaultButtnSelected);
    }

    public void ButtonSelected(int buttonIndex)
    {
        for (int i = 0; i < _bottomButtons.Length; i++)
        {
            var i1 = i;
            float scaleFactor = buttonIndex == i ? 1.2f : 1;
            Vector3 moveFactor = buttonIndex == i ? Vector3.up * 15 : Vector3.zero;
            _bottomButtons[i].transform.GetChild(1).DOScale(Vector3.one*scaleFactor, 0.3f ).SetEase(Ease.OutBack);
            _bottomButtons[i].transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPos(moveFactor, 0.3f ).SetEase(Ease.OutBack);
            _bottomButtons[i].interactable = false;
        }

        DOVirtual.DelayedCall(0.27f, EnableAllButtons);
    }

    void EnableAllButtons()
    {
        for (int i = 0; i < _bottomButtons.Length; i++)
        {
            _bottomButtons[i].interactable = true;
        }
    }
}
