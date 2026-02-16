using UnityEngine;

public static class Extentions
{
    public static void SetActiveState(this GameObject go, bool value)
    {
        if (go) // Unity-style null check
            go.SetActive(value);
    }
}
