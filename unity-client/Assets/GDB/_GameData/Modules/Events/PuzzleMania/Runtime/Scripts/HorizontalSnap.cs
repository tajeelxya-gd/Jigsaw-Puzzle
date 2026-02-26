using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HorizontalSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject[] _placeholders;
    public ScrollRect scrollRect;
    public float snapSpeed = 10f;

    private RectTransform contentRect;
    private float[] positions;
    private int imageCount;
    private bool isDragging = false;
    private int activeImageIndex = 0; // Stores the current active image index

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
            float currentPos = scrollRect.horizontalNormalizedPosition;
            float newPos = Mathf.Lerp(currentPos, closestPos, Time.deltaTime * snapSpeed);
            scrollRect.horizontalNormalizedPosition = newPos;
        }
    }

    float FindClosestPosition()
    {
        float scrollPos = scrollRect.horizontalNormalizedPosition;
        float closest = positions[0];
        float minDistance = Mathf.Abs(scrollPos - closest);

        for (int i = 1; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(scrollPos - positions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = positions[i];
            }
        }
        return closest;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Find and store the active image index
        float scrollPos = scrollRect.horizontalNormalizedPosition;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(scrollPos - positions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                activeImageIndex = i;
            }
        }

        Debug.Log("Active image index: " + activeImageIndex);
        SetActivePlaceHolders();
    }

    // Optional: Method to access the active index from other scripts
    public int GetActiveImageIndex()
    {
        return activeImageIndex;
    }

    private void SetActivePlaceHolders()
    {
        for (var i = 0; i < _placeholders.Length; i++)
        {
            var active = i == activeImageIndex;
            _placeholders[i].SetActive(active);
        }
    }
}