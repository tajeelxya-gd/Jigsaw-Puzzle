using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HorizontalScrollSnapper : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject[] _contentItems;
    private RectTransform[] _targets;

    private void Awake()
    {
        if (scrollRect == null) Debug.LogError("ScrollRect not assigned!");
        if (_contentItems == null || _contentItems.Length == 0) Debug.LogError("_contentItems not assigned!");
    }

    private void Start()
    {
        _targets = new RectTransform[_contentItems.Length];
        for (int i = 0; i < _contentItems.Length; i++)
        {
            _targets[i] = _contentItems[i].GetComponent<RectTransform>();
        }
    }

    public void SnapToItemInstant(int index)
    {
        if (index < 0 || index >= _targets.Length) return;

        Canvas.ForceUpdateCanvases();

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        Vector3[] contentCorners = new Vector3[4];
        content.GetWorldCorners(contentCorners);

        Vector3[] targetCorners = new Vector3[4];
        _targets[index].GetWorldCorners(targetCorners);

        float distanceFromLeft = targetCorners[0].x - contentCorners[0].x; // horizontal offset
        float contentWidth = contentCorners[3].x - contentCorners[0].x;
        float viewportWidth = viewport.rect.width;

        float normalizedX = Mathf.Clamp01(distanceFromLeft / (contentWidth - viewportWidth));

        // Instant snap
        scrollRect.horizontalNormalizedPosition = normalizedX;
    }
}