using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class OnBoardingUtilityComponent : MonoBehaviour
{
    [Title("OnBoarding")]
    [SerializeField] private bool ignoreOnBoarding = false;
    [SerializeField] private OnBoardingConfig.OnBoardingType _onBoardingType;
    [SerializeField] private OnBoardingMainMenue.InfoAlignment infoBoxAlignment;
    [SerializeField] private RectTransform focusTargetPosition;
    [SerializeField] private RectTransform focusHandPosition;
    [SerializeField] private RectTransform infoBoxPosition;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button _unlockedButton;
    [SerializeField] private Button _lockedButton;
    [SerializeField] private string infoText ="Puzzle Rush Unlocked!" ;
    [Button("Clear Onboarding")]
    public void ClearOnBoarding()
    {
        OnBoardingConfig.ClearOnBoarding(_onBoardingType);
    }
    
    [Button("Set Onboarding")]
    public void SetOnBoardingDone()
    {
        OnBoardingConfig.SetOnBoardingDone(_onBoardingType);
    }
    private Transform _parentObject;

    private void Awake()
    {
        _parentObject = focusTargetPosition.transform.parent;
        _unlockedButton.onClick.AddListener(OnButtonClicked);
        
    }
    void OnButtonClicked()
    {
        if (!OnBoardingConfig.HasOnBoardingActivatedBefore(_onBoardingType))
        {
            focusTargetPosition.transform.SetParent(_parentObject, true);
            OnBoardingConfig.SetOnBoardingDone(_onBoardingType);
            SignalBus.Publish(new ShowOnBoardingSignal { DoActivate = false });
            infoButton.onClick.Invoke();
          
        }
    }
    
    void InitOnBoarding()
    {
        if (ignoreOnBoarding) return;
       
        Debug.LogError("Onboarding requested!!");
        if (!OnBoardingConfig.HasOnBoardingActivatedBefore(_onBoardingType))
        {
            if(_unlockedButton)
                _unlockedButton.gameObject.SetActive(false);
            if(_lockedButton)
                _lockedButton.gameObject.SetActive(true);
            UnityAction<Action> onBoardingAction = finish =>
            {
                if(_unlockedButton)
                    _unlockedButton.gameObject.SetActive(true);
                if(_lockedButton)
                    _lockedButton.gameObject.SetActive(false);
                SignalBus.Publish(new ShowOnBoardingSignal
                {
                    DoActivate = true,
                    FocusTargetTransform = focusTargetPosition,
                    FocusHandPosition = focusHandPosition,
                    InfoPanelPosition = infoBoxPosition,
                    Alignment = infoBoxAlignment,
                    InfoText = infoText
                });
            };
            Debug.LogError(" onboarding panel activated");
            PopCommandExecutionResponder.AddCommand(new OnBoardingMenuCommand(PopCommandExecutionResponder.PopupPriority.High, onBoardingAction));
                
        }
    }
    
}
