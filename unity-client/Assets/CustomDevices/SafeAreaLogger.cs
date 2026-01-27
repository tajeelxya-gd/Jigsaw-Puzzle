using UnityEngine;

public sealed class SafeAreaLogger : MonoBehaviour
{
    private void Start()
    {
        Rect safeArea = Screen.safeArea;

        Debug.Log($"--- Safe Area Stats ---");
        Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
        Debug.Log($"X: {safeArea.x}");
        Debug.Log($"Y: {safeArea.y}");
        Debug.Log($"Width: {safeArea.width}");
        Debug.Log($"Height: {safeArea.height}");
        Debug.Log($"---------------------------------------");
    }
}