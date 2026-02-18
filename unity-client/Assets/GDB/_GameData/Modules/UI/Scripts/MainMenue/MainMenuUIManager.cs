using System;
using System.Collections;
using DG.Tweening;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private Button[] mainHudButtons;
    [SerializeField] SettingsManager settingsManager;
    [SerializeField] LevelStateMainMenue levelStateMainMenue;
    private bool currentRaycastState;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        SignalBus.Reset();
        PopCommandExecutionResponder.ClearAllCommands();
        SetRaycastState(false);
        currentHaultTime = 3;
    }


    IEnumerator SetUpRoutine()
    {
        int totalLevelCount = GlobalService.MaxLevel;

        int currentLevel = GlobalService.GameData.Data.LevelIndex;
        int nextLevel = GetWrappedLevel(currentLevel);

        ResourceRequest request = Resources.LoadAsync<TextAsset>($"Levels/Level {nextLevel}");

        while (!request.isDone)
        {
            // You can update a loading bar here if you want!
            yield return null;
        }

        TextAsset json = request.asset as TextAsset;

        if (json != null)
        {
            LevelData leveldata = ScriptableObject.CreateInstance<LevelData>();
            JsonUtility.FromJsonOverwrite(json.text, leveldata);

            settingsManager.Inject(leveldata);
            levelStateMainMenue.Inject(leveldata);

            // Clean up the string memory
            Resources.UnloadAsset(json);
        }
    }

    const int FIRST_REPEAT_LEVEL = 101;
    int GetWrappedLevel(int level)
    {
        int _totalLevelCount = GlobalService.MaxLevel;
        if (level <= GlobalService.MaxLevel)
            return level;

        int repeatRange = _totalLevelCount - FIRST_REPEAT_LEVEL + 1;
        return FIRST_REPEAT_LEVEL + ((level - _totalLevelCount - 1) % repeatRange);
    }
    private void Start()
    {
        DOVirtual.DelayedCall(1, PopCommandExecutionResponder.ExecuteNext);
        StartCoroutine(CommandStateWatcher());
        StartCoroutine(SetUpRoutine());
        SignalBus.Subscribe<OnInAppBuySignal>(OnInAppBuySignal);
        SignalBus.Subscribe<OnHaltMainMenuExecutionSignal>(OnHaltExecutionSignal);
    }

    void OnInAppBuySignal(ISignal signal)
    {
        currentHaultTime = 3;
    }

    void OnHaltExecutionSignal(ISignal signal)
    {
        currentHaultTime = 3;

    }

    private void Update()
    {
        if (currentHaultTime > 0)
            currentHaultTime -= Time.deltaTime;
        currentHaultTime = Mathf.Clamp(currentHaultTime, 0, Mathf.Infinity);
    }

    [ReadOnly, SerializeField]
    private bool haultStateActive => currentHaultTime > 0;
    [ReadOnly, SerializeField]
    private float currentHaultTime = 3;

    private void SetRaycastState(bool state)
    {
        if (currentRaycastState == state || mainHudButtons == null)
            return;

        currentRaycastState = state;

        for (int i = 0; i < mainHudButtons.Length; i++)
        {
            if (mainHudButtons[i] != null && mainHudButtons[i].image)
                mainHudButtons[i].enabled = state;
        }
    }

    private System.Collections.IEnumerator CommandStateWatcher()
    {
        var wait = new WaitForSeconds(0.1f);
        while (true)
        {

            SetRaycastState(!PopCommandExecutionResponder.DoCommandsExists() && !haultStateActive);
            yield return wait;
        }
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnInAppBuySignal>(OnInAppBuySignal);
        SignalBus.Unsubscribe<OnHaltMainMenuExecutionSignal>(OnHaltExecutionSignal);
    }
}

public class OnHaltMainMenuExecutionSignal : ISignal { }