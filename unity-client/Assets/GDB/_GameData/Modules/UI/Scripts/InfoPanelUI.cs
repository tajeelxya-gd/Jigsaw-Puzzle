using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class InfoPanelUI : MonoBehaviour
{
    [SerializeField] private Transform[] _elements;
    [SerializeField] private Button _continueButton;
    [SerializeField] private CanvasGroup _root_Cg;
    [SerializeField] private float animationDuration = 0.15f;

    private void Awake()
    {
        _continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void OnEnable()
    {
        OnShowPanel();
    }

    void Reset()
    {
        foreach (var element in _elements)
            element.gameObject.SetActive(false);
        _root_Cg.alpha = 1;
        transform.localScale = Vector3.one;
    }
    private Sequence sequence;

    void OnShowPanel()
    {
        if (sequence != null)
            sequence.Kill();
        sequence = DOTween.Sequence();
        //  sequence.Append()
        foreach (Transform element in _elements)
        {
            sequence.Append(
                element.DOScale(Vector3.one, animationDuration)
                    .From(Vector3.zero)
                    .SetEase(Ease.OutBack)
            ).JoinCallback(() =>
            {
                element.gameObject.SetActive(true);
            });
        }
    }

    void OnContinueButtonClicked()
    {
        transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => { gameObject.SetActive(false); });
    }

    private void OnDisable()
    {
        sequence?.Kill();
        Reset();
    }
}
