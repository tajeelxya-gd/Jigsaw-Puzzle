using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardControllerUI : MonoBehaviour
{
    [Title("OnBoarding")]
    [SerializeField] private OnBoardingConfig.OnBoardingType _onBoardingType;
    [SerializeField] private GameObject _lockedButton;
    [SerializeField] private GameObject _unlockedButton;
    [SerializeField] private BulkPopUpEffect medalsBulkEffect;
    private const string Key = "LEADERBOARD_MISSIONS_ONBOARDING";

    private void Start()
    {
        _gameData = GlobalService.GameData;
        UpdateUI();
        DOVirtual.DelayedCall(Time.deltaTime * 10, LookForOnBoardingPanel);
    }


    [Button("Clear Onboarding")]
    public void ClearOnBoarding()
    {
        OnBoardingConfig.ClearOnBoarding(_onBoardingType);
    }
    void LookForOnBoardingPanel()
    {
        if (_unlockedButton.activeInHierarchy)
        {
            if (!OnBoardingConfig.HasOnBoardingActivatedBefore(_onBoardingType))
            {
                PopCommandExecutionResponder.AddCommand(new LeaderBoardUnlockCommand(PopCommandExecutionResponder.PopupPriority.Low, execution => { medalsBulkEffect.Generate(null, 10); }));
                OnBoardingConfig.SetOnBoardingDone(_onBoardingType);
            }
        }
    }
    void UpdateUI()
    {
        _lockedButton.gameObject.SetActive(_gameData.Data.LevelIndex < (int)_onBoardingType);
        _unlockedButton.gameObject.SetActive(_gameData.Data.LevelIndex >= (int)_onBoardingType);
    }
    private GameData _gameData;
}
