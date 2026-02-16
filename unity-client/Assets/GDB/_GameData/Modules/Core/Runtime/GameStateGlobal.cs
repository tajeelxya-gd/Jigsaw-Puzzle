using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameStateGlobal
{
    public static bool AdShownSuccessfully = false;
    public static bool IsSandBox = true;
    public static bool IsGamePlay => SceneManager.GetActiveScene().name == "Game";
    public static bool GreatRaceRequestedAlready = false;
}
