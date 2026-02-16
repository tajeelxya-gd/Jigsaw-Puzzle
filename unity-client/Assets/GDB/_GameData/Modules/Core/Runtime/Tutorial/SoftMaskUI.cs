using System;
using UnityEngine;
using DG.Tweening;
public class SoftMaskUI : MonoBehaviour
{
    [SerializeField] private GameObject _maskRoot;
    [SerializeField] private RectTransform _mask;
    [SerializeField] private RectTransform _handAnimation;


    private void Start()
    {
        SignalBus.Subscribe<OnTutorialActivated>(OnTutorialActivated);
    }

    public void ShowFocusesMask(RectTransform target, RectTransform handRect, bool ShowHand, bool doAnchoredPosition = true)
    {
        _maskRoot.SetActive(true);
        _handAnimation.gameObject.SetActive(ShowHand);
        _mask.gameObject.SetActive(true);
        if (doAnchoredPosition)
            _mask.anchoredPosition = target.anchoredPosition;
        else
            _mask.transform.position = target.position;
        _mask.sizeDelta = target.sizeDelta;

        _handAnimation.position = handRect.position;
        _handAnimation.transform.rotation = handRect.rotation;
        _mask.transform.DOScale(1, 0.25f).From(5).SetEase(Ease.Linear);
        DOVirtual.DelayedCall(Time.deltaTime, () => _handAnimation.transform.SetAsLastSibling());
    }

    public void HideMask()
    {
        _maskRoot.SetActive(false);
        _handAnimation.gameObject.SetActive(false);
    }

    public void OnTutorialActivated(OnTutorialActivated signal)
    {
        if (signal.IsActivated == false)
            HideMask();
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnTutorialActivated>(OnTutorialActivated);

    }
}
