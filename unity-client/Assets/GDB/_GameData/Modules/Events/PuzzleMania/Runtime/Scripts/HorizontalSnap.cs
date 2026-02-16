using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float snapSpeed = 10f;

    private RectTransform contentRect;
    private float[] positions;
    private int imageCount;
    private bool isDragging = false;

    void Start()
    {
        contentRect = scrollRect.content;
        imageCount = contentRect.childCount;
        positions = new float[imageCount];

        float step = 1f / (imageCount - 1);
        for (int i = 0; i < imageCount; i++)
        {
            positions[i] = step * i;
        }
    }

    void Update()
    {
        if (!isDragging)
        {
            float closestPos = FindClosestPosition();
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition,
                new Vector2(-closestPos * GetContentWidth(), contentRect.anchoredPosition.y),
                Time.deltaTime * snapSpeed);
        }
    }

    float FindClosestPosition()
    {
        float scrollNormalized = scrollRect.horizontalNormalizedPosition;
        float closest = positions[0];
        float minDistance = Mathf.Abs(scrollNormalized - closest);

        foreach (float pos in positions)
        {
            float distance = Mathf.Abs(scrollNormalized - pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = pos;
            }
        }
        return closest;
    }

    float GetContentWidth()
    {
        return contentRect.rect.width - scrollRect.viewport.rect.width;
    }

    public void OnBeginDrag()
    {
        isDragging = true;
    }

    public void OnEndDrag()
    {
        isDragging = false;
    }
}