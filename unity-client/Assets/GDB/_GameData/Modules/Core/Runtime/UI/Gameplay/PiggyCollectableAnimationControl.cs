using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PiggyCollectableAnimationControl : MonoBehaviour
{
    [SerializeField] private RectTransform _parentRect;
    [SerializeField] private TextMeshProUGUI _goalTxt;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Button]
    public void Animate(int amount, Action CallBack = null)
    {
        _canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        StartCoroutine(AnimateRoutine(amount, CallBack));
    }
    private IEnumerator AnimateRoutine(int amount, Action CallBack)
    {
        yield return new WaitUntil(() => !TutorialManager.IsTutorialActivated);
        _canvasGroup.alpha = 1;
        _parentRect.localScale = Vector3.one * 0.5f;
        _parentRect.anchoredPosition = new Vector2(0, -200);

        _goalTxt.text = amount.ToString();

        _parentRect.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .From(0.5f);

        _parentRect.DOAnchorPos(Vector2.zero, 0.5f)
            .SetEase(Ease.OutCubic)
            .From(new Vector2(0, -200));

        _parentRect.DOAnchorPos(new Vector2(0, 460), 0.75f)
            .SetEase(Ease.OutCubic)
            .From(Vector2.zero)
            .SetDelay(0.9f);

        DOVirtual.DelayedCall(1.3f, () =>
        {
            CallBack?.Invoke();
        });

        _canvasGroup.DOFade(0, 0.3f)
            .SetEase(Ease.Linear)
            .SetDelay(1.5f)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        yield return null;
    }
}