using System;
using DG.Tweening;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
public class LevelStateMainMenue : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _lvlNo_txt, _lvlDifficulty_txt;
    [SerializeField] Image lvlDifficulty_img;
    [SerializeField] private Sprite _hardLvl_sprite, _superhardLvl_sprite;
    private GameSoundController _gameSoundController;
    [SerializeField] LevelMainButton _levelMainButton;
    [SerializeField] private Button _levelSelectionButton;
    private LevelData _leveldata;
    public void Inject(LevelData levelData)
    {
        _leveldata = levelData;
        LoadLevelStatus();
        _levelSelectionButton.interactable = false;
        DOVirtual.DelayedCall(2, LoadButtonEnable);
    }

    void LoadButtonEnable() => _levelSelectionButton.interactable = true;

    const int FIRST_REPEAT_LEVEL = 101;
    int GetWrappedLevel(int level)
    {
        int _totalLevelCount = GlobalService.MaxLevel;
        if (level <= GlobalService.MaxLevel)
            return level;

        int repeatRange = _totalLevelCount - FIRST_REPEAT_LEVEL + 1;
        return FIRST_REPEAT_LEVEL + ((level - _totalLevelCount - 1) % repeatRange);
    }
    void LoadLevelStatus()
    {
        GameData gameData = GlobalService.GameData;
        int currentLevel = gameData.Data.LevelNumber;
        _lvlNo_txt.text = $"Level {currentLevel + 1}";
        _lvlDifficulty_txt.text = _leveldata.levelType.ToString();
        _gameSoundController = new GameSoundController(_leveldata);
        _gameSoundController.PlayBG();
        _levelMainButton.Inject(_leveldata);

        switch (_leveldata.levelType)
        {
            case LevelType.Easy:
                break;
            case LevelType.Hard:
                lvlDifficulty_img.sprite = _hardLvl_sprite;
                break;
            case LevelType.SuperHard:
                lvlDifficulty_img.sprite = _superhardLvl_sprite;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        SignalBus.Publish(new OnGraphicsUtilityActivatedSignal { LvlType = _leveldata.levelType });

    }
}
