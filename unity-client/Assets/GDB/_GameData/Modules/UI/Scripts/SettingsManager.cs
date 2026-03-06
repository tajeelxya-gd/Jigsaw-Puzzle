using System.Collections;
using DG.Tweening;
using UniTx.Runtime.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private protected GameObject _settingPanel;
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private Image musicButtonImage;
    [SerializeField] private Image hapticsButtonImage;
    [SerializeField] private Sprite onIcon;
    [SerializeField] private Sprite offIcon;
    // private DataBaseService<GameSettings> dbService;
    // private GameSettings settings;
    [SerializeField] private bool _isGamePlay = false;
    [SerializeField] private Button _homeButton;
    private GameData _gameData;
    ITimeService timeService_freeTimer;
    ITimeService timeService_resetHealthTimer;
    private bool _isInfiniteTimerActive = false;

    private LevelData _leveldata;


    public void Inject(LevelData levelData)
    {
        _leveldata = levelData;
        _gameData = GlobalService.GameData;
        SetUp();
    }

    void SetUp()
    {
        timeService_freeTimer = new RealTimeService(PlayerHealthTimerType.InfiniteHealthTimer.ToString(), OnTimerEnded);
        timeService_resetHealthTimer =
            new RealTimeService(PlayerHealthTimerType.ResetHealthTimer.ToString(), OnTimerEndedResetHealth);
        _isInfiniteTimerActive = timeService_freeTimer.IsRunning();
        UpdateAllUI();
    }
    public void ToggleSound()
    {
        _gameData.Data.SoundOn = !_gameData.Data.SoundOn;
        AudioController.PlaySFX(AudioType.SettingButtonClick);
        SaveAndUpdate();
    }

    public void ToggleMusic()
    {
        _gameData.Data.MusicOn = !_gameData.Data.MusicOn;
        if (_gameData.Data.MusicOn)
        {
            if (_leveldata.levelType == LevelType.Easy)
            {
                AudioController.PlayBG(AudioType.BGSimple);
            }
            else if (_leveldata.levelType == LevelType.Hard)
            {
                AudioController.PlayBG(AudioType.BGHard);
            }
            else
            {
                AudioController.PlayBG(AudioType.BGSuperHard);
            }
        }
        else
        {
            AudioController.StopBG();
        }
        AudioController.PlaySFX(AudioType.SettingButtonClick);
        SaveAndUpdate();
    }

    public void ToggleHaptics()
    {
        AudioController.PlaySFX(AudioType.SettingButtonClick);
        _gameData.Data.HapticsOn = !_gameData.Data.HapticsOn;
        SaveAndUpdate();
    }

    private void SaveAndUpdate()
    {
        UniAudio.IsMusicOn = _gameData.Data.MusicOn;
        UniAudio.IsSoundOn = _gameData.Data.SoundOn;
        _gameData.Save();
        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        soundButtonImage.sprite = _gameData.Data.SoundOn ? onIcon : offIcon;
        musicButtonImage.sprite = _gameData.Data.MusicOn ? onIcon : offIcon;
        hapticsButtonImage.sprite = _gameData.Data.HapticsOn ? onIcon : offIcon;
        if (_isGamePlay)
        {
            _homeButton.onClick.AddListener(GoToHomeButton);
            _homeButton.gameObject.SetActive(true);
        }
        else
        {
            _homeButton.gameObject.SetActive(false);
        }
    }

    private void GoToHomeButton()
    {
        //Debug.LogError("settings home");
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        if (_isInfiniteTimerActive)
        {
            Time.timeScale = 1;
            _settingPanel.SetActive(false);
            SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = true, FakeLoadTime = 2, OnFakeLoadCompleteEven = LoadSceneToHome });
        }
        else
        {
            if (GlobalService.GameData.Data.AvailableLives == 0)
            {
                Time.timeScale = 1;
                _settingPanel.SetActive(false);
                SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = true, FakeLoadTime = 2, OnFakeLoadCompleteEven = LoadSceneToHome });
            }
            else
            {
                SignalBus.Publish(new AreYouSurePanelSignal());
            }
        }
    }
    void LoadSceneToHome()
    {
        SceneManager.LoadScene(1);
    }

    public virtual void OpenSettingPanel()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _settingPanel.transform.localScale = Vector3.one;
        _settingPanel.SetActive(true);
    }

    public virtual void CloseSettingPanel()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _settingPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => { _settingPanel.gameObject.SetActive(false); });
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        _settingPanel.SetActive(false);
    }
    void OnTimerEnded() { }

    void OnTimerEndedResetHealth() { }
}

// [System.Serializable]
// public class GameSettings
// {
//     public bool SoundOn = true;
//     public bool MusicOn = true;
//     public bool HapticsOn = true;
// }