using UniTx.Runtime;
using UnityEngine;

namespace PuzzleTemplate.Runtime
{
    [ExecuteAlways]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2 _lastScreenSize = Vector2.zero;

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            Refresh();
        }

        private void Update()
        {
            // The simulator sometimes changes resolution without changing safe area immediately
            if (_lastSafeArea != Screen.safeArea || _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_rectTransform == null) return;

            var safeArea = Screen.safeArea;

            UniStatics.LogInfo($"X: {safeArea.x}", this, Color.blanchedAlmond);
            UniStatics.LogInfo($"Y: {safeArea.y}", this, Color.blanchedAlmond);

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2(Screen.width, Screen.height);

            // Calculation for UV space
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // Set offsets to zero to ensure it stretches to the anchors
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}