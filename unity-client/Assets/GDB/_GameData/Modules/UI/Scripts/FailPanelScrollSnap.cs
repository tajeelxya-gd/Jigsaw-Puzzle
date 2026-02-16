using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class FailPanelScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;

    [Header("Settings")]
    [SerializeField] private float snapDuration = 0.25f;
    [SerializeField] private float swipeThreshold = 60f;

    private int currentIndex;
    private float dragStartX;
    private float[] snapX;

    IEnumerator Start()
    {
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.inertia = false;
        scrollRect.elasticity = 0f;

        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        int count = content.childCount;
        snapX = new float[count];

        // Calculate snap positions relative to viewport
        for (int i = 0; i < count; i++)
        {
            RectTransform child = content.GetChild(i).GetComponent<RectTransform>();
            // The position of the child’s left edge relative to content pivot
            float childPos = content.anchoredPosition.x + (child.localPosition.x - child.rect.width * child.pivot.x);
            snapX[i] = -childPos;
        }

        // Start at first child
        currentIndex = 0;
        content.anchoredPosition = new Vector2(snapX[0], content.anchoredPosition.y);
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<OnBundleButtonClick>(SnapToIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartX = eventData.position.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float delta = eventData.position.x - dragStartX;

        if (Mathf.Abs(delta) < swipeThreshold)
        {
            SnapToClosest();
            return;
        }

        if (delta < 0)
            currentIndex++;
        else
            currentIndex--;

        currentIndex = Mathf.Clamp(currentIndex, 0, snapX.Length - 1);
        Snap();
    }

    private void SnapToClosest()
    {
        float posX = content.anchoredPosition.x;
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < snapX.Length; i++)
        {
            float distance = Mathf.Abs(snapX[i] - posX);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentIndex = closestIndex;
        Snap();
    }

    private void Snap()
    {
        StopAllCoroutines();
        StartCoroutine(SmoothSnap());
    }

    private IEnumerator SmoothSnap()
    {
        Vector2 start = content.anchoredPosition;
        Vector2 target = new Vector2(snapX[currentIndex], start.y);

        float t = 0f;
        while (t < snapDuration)
        {
            t += Time.unscaledDeltaTime;
            content.anchoredPosition = Vector2.Lerp(start, target, t / snapDuration);
            yield return null;
        }

        content.anchoredPosition = target;
        SignalBus.Publish(new OnBundleImageUpdate { _bundleImageIndex = currentIndex });
    }
    private void SnapToIndex(OnBundleButtonClick signal)
    {
        if (snapX == null || snapX.Length == 0)
            return;

        currentIndex = Mathf.Clamp(signal._bundleIndex, 0, snapX.Length - 1);
        Snap();
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnBundleButtonClick>(SnapToIndex);
    }
}

public class OnBundleButtonClick : ISignal
{
   public int _bundleIndex;
}

public class OnBundleImageUpdate : ISignal
{
    public int _bundleImageIndex;
}