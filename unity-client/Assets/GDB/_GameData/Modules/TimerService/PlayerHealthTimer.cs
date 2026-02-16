using System;
using System.Globalization;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UniTx.Runtime;
using UnityEngine;

public enum PlayerHealthTimerType
{
    InfiniteHealthTimer,
    ResetHealthTimer
}

public class PlayerHealthTimer : MonoBehaviour
{
    ITimeService timeService_freeTimer;
    ITimeService timeService_resetHealthTimer;

    [SerializeField] private TextMeshProUGUI _infiniteTimer_Txt;
    [SerializeField] private TextMeshProUGUI _livesAnim_Txt;
    [SerializeField] private TextMeshProUGUI _playerLives_Txt;
    [SerializeField] private TextMeshProUGUI _healthResetTimer_Txt;
    [SerializeField] private GameObject _infiniteIcon;
    private CanvasGroup livesCanvasGroup;
    private GameData gameData;
    private int resetHealthTimeInSec = 1500;
    [SerializeField] private bool _isGamePlay = false;

    private void Awake()
    {
        if (!_isGamePlay)
        {
            livesCanvasGroup = _livesAnim_Txt.GetComponent<CanvasGroup>();
        }
        SignalBus.Subscribe<OnHealthUpdateSignal>(AddInfiniteLivesTime);
    }

    void LoadData()
    {
        gameData = GlobalService.GameData;
        if (gameData == null)
        {
            gameData = new GameData();
            gameData.SetupData();
        }
    }

    private DateTime previousResetHealthTime;

    void Start()
    {
        LoadData();
        timeService_freeTimer =
            new RealTimeService(nameof(PlayerHealthTimerType.InfiniteHealthTimer), OnTimerEndedInfiniteHealth);
        timeService_resetHealthTimer =
            new RealTimeService(nameof(PlayerHealthTimerType.ResetHealthTimer), OnTimerEndedResetHealth);
        if (_isGamePlay) return;
        UpdateUI();
    }

    void OnLowHealth()
    {
        if (gameData != null && gameData.Data.AvailableLives < 5)
        {
            if (timeService_resetHealthTimer != null && !timeService_resetHealthTimer.IsRunning())
            {
                timeService_resetHealthTimer.StartTimer(resetHealthTimeInSec);
                gameData.Data.PreviousHealthResetTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                UniStatics.LogInfo("reset Timer start set successfully");
                gameData.Save();
            }
            //timeService_resetHealthTimer.StartTimer(1500);
        }
    }

    void UpdateUI()
    {
        _playerLives_Txt.gameObject.SetActive(!timeService_freeTimer.IsRunning());
        _healthResetTimer_Txt.gameObject.SetActive(!timeService_freeTimer.IsRunning());
        _infiniteIcon.gameObject.SetActive(timeService_freeTimer.IsRunning());
        _infiniteTimer_Txt.gameObject.SetActive(timeService_freeTimer.IsRunning());
        _playerLives_Txt.text = gameData.Data.AvailableLives.ToString();
        _infiniteTimer_Txt.text = timeService_freeTimer.GetFormattedTimeMinutes();
        if (timeService_resetHealthTimer != null && !timeService_resetHealthTimer.IsRunning())
            _healthResetTimer_Txt.text = gameData.Data.AvailableLives >= 5
                ? "Full"
                : timeService_resetHealthTimer.GetFormattedTimeMinutes();
        else
            _healthResetTimer_Txt.text = timeService_resetHealthTimer.GetFormattedTimeMinutes();
    }

    [Button("Add Time Test")]
    public void AddTimerTest(int minutes)
    {
        SignalBus.Publish(new OnHealthUpdateSignal { TimeToAdd = minutes });
    }

    private void AddInfiniteLivesTime(OnHealthUpdateSignal signal)
    {
        // gameData.Data.AvailableLives += signal.Amount;
        if (!timeService_freeTimer.IsRunning())
            timeService_freeTimer.StartTimer(signal.TimeToAdd * 60);
        else
            timeService_freeTimer.ExtendTimer(signal.TimeToAdd);
        PlayAnimation(_livesAnim_Txt, livesCanvasGroup, _infiniteTimer_Txt.transform.position, signal.TimeToAdd);
        // SaveAndRefresh();
    }

    private void PlayAnimation(
        TextMeshProUGUI animText,
        CanvasGroup canvasGroup,
        Vector3 startPosition,
        int amount)
    {
        animText.transform.position = startPosition;
        animText.color = amount > 0 ? Color.green : Color.red;
        animText.text = amount > 0 ? $"+{amount}" : amount.ToString();

        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0f, 2f);

        animText.rectTransform.anchoredPosition = Vector2.zero;
        animText.rectTransform.DOAnchorPos(Vector2.down * 50f, 2f);
    }

    void OnTimerEndedInfiniteHealth()
    {
    }

    void OnTimerEndedResetHealth()
    {
        Debug.Log("Reset Timer has ended!");
        int totalLives = gameData.Data.AvailableLives;

        if (totalLives < 5)
        {
            if (DateTime.TryParse(gameData.Data.PreviousHealthResetTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime savedTime))
            {
                double secondsPassed = (DateTime.UtcNow - savedTime).TotalSeconds;
                Debug.Log("Total Seconds Passed Since Last Save: " + secondsPassed);
                int livesToAdd = Mathf.FloorToInt((float)(secondsPassed / resetHealthTimeInSec));
                livesToAdd = Mathf.Clamp(livesToAdd, 1, 5 - totalLives);
                gameData.Data.AvailableLives += livesToAdd;
                gameData.Data.AvailableLives = Mathf.Clamp(gameData.Data.AvailableLives, 0, 5);
                Debug.Log($"Added {livesToAdd} lives based on time elapsed.");
            }
            else
            {
                gameData.Data.AvailableLives += 1;
                Debug.LogWarning("Could not parse PreviousHealthResetTime string. Defaulting to +1 life.");
            }
        }
        gameData.Save();
    }

    // Update is called once per frame
    void Update()
    {
        timeService_freeTimer.Update();
        timeService_resetHealthTimer.Update();
        OnLowHealth();
        if (_isGamePlay) return;
        UpdateUI();
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnHealthUpdateSignal>(AddInfiniteLivesTime);
        SaveTimer();
    }

    void SaveTimer()
    {
        if (timeService_resetHealthTimer != null)
            timeService_resetHealthTimer.SaveTimer();
        if (timeService_freeTimer != null)
            timeService_freeTimer.SaveTimer();
    }

    private void OnDestroy()
    {
        SaveTimer();
    }

    [Button]
    public void Test(int time)
    {
        SignalBus.Publish(new OnHealthUpdateSignal() { TimeToAdd = time });
    }
}