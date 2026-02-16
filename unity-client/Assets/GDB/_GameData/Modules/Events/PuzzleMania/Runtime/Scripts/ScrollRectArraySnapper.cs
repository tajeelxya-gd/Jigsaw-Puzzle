using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

public class ScrollRectArraySnapper : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject[] _contentItems;
    private RectTransform[] _target;

    private void Start()
    {
        _target = new RectTransform[_contentItems.Length];
        for (int i = 0; i < _contentItems.Length; i++)
        {
            _target[i]= _contentItems[i].GetComponent<RectTransform>();
        }
    }

    public void SnapToItem(int index, float duration = 0.5f)
    {
        if (index < 0 || index >= _contentItems.Length) return;
        StartCoroutine(SnapRoutine(index, duration));
    }

    private IEnumerator SnapRoutine(int index, float duration)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        Vector3[] contentCorners = new Vector3[4];
        content.GetWorldCorners(contentCorners);

        Vector3[] targetCorners = new Vector3[4];
        _target[index].GetWorldCorners(targetCorners);

        float distanceFromTop = contentCorners[1].y - targetCorners[1].y; 

        float contentHeight = contentCorners[1].y - contentCorners[0].y;
        float viewportHeight = viewport.rect.height;

        float normalizedY = Mathf.Clamp01(distanceFromTop / (contentHeight - viewportHeight));

        scrollRect.DOVerticalNormalizedPos(1f - normalizedY, duration).SetEase(Ease.OutCubic);
    }
}