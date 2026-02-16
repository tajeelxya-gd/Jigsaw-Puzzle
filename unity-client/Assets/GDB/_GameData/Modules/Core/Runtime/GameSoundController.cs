using UnityEngine;

public class GameSoundController
{
    private LevelData _currentLevelData;
    public GameSoundController(LevelData levelController)
    {
        _currentLevelData = levelController;
    }
    public void PlayBG()
    {
        switch (_currentLevelData.levelType)
        {
            case LevelType.Hard: AudioController.PlayBG(AudioType.BGHard); break;
            case LevelType.SuperHard: AudioController.PlayBG(AudioType.BGSuperHard); break;
            default: AudioController.PlayBG(AudioType.BGSimple); break;
        }
    }
}