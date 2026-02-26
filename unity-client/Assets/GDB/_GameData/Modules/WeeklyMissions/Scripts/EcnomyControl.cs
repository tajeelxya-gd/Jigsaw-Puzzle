using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EconomyControl : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Animation UI")]
    [SerializeField] private TextMeshProUGUI coinsAnimText;
    [SerializeField] private TextMeshProUGUI livesAnimText;

    private GameData gameData;
    private CanvasGroup coinsCanvasGroup;
    private CanvasGroup livesCanvasGroup;

    private void Awake()
    {

        coinsCanvasGroup = coinsAnimText.GetComponent<CanvasGroup>();
        livesCanvasGroup = livesAnimText.GetComponent<CanvasGroup>();

    }


    void ShowWinCoinsPop()
    {
        gameData.Data.BackFromWin = false;
        PopCommandExecutionResponder.AddCommand(new GameplayRewardShowCommand(
            PopCommandExecutionResponder.PopupPriority.Low,
            execution =>
            {
                IBulkPopService popBulkService = GlobalService.IBulkPopService;
                popBulkService.PlayEffect(0, PopBulkService.BulkPopUpServiceType.Coins,
                    new Vector2(Screen.width / 2f, Screen.height / 2f), 10, CloseExecutionCommand);
                DOVirtual.DelayedCall(0.25f, CloseExecutionCommand);
            }
        ));
    }

    void CloseExecutionCommand()
    {
        Debug.Log("CloseOnBoardingCommandIfYes :: " + PopCommandExecutionResponder.HasCommand<GameplayRewardShowCommand>());
        //  if (PopCommandExecutionResponder.HasCommand<GameplayRewardShowCommand>())
        PopCommandExecutionResponder.RemoveCommand<GameplayRewardShowCommand>();
    }


    private void Start()
    {
        SignalBus.Subscribe<OnCoinsUpdateSignal>(OnUpdateCoins);
        SignalBus.Subscribe<OnHammerUpdateSignal>(OnUpdateHammer);
        SignalBus.Subscribe<OnWandUpdateSignal>(OnUpdateWand);
        SignalBus.Subscribe<OnSlotPopperUpdateSignal>(OnUpdateSlotPopper);
        SignalBus.Subscribe<OnMagnetUpdateSignal>(OnUpdateMagnet);
        LoadData();
        UpdateUI();

        GameData gameData = GlobalService.GameData;
        if (gameData.Data.BackFromWin)
            ShowWinCoinsPop();

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

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnCoinsUpdateSignal>(OnUpdateCoins);
        SignalBus.Unsubscribe<OnHammerUpdateSignal>(OnUpdateHammer);
        SignalBus.Unsubscribe<OnWandUpdateSignal>(OnUpdateWand);
        SignalBus.Unsubscribe<OnSlotPopperUpdateSignal>(OnUpdateSlotPopper);
        SignalBus.Unsubscribe<OnMagnetUpdateSignal>(OnUpdateMagnet);
    }

    private void UpdateUI()
    {
        coinsText.text = gameData.Data.Coins.ToString();
        healthText.text = gameData.Data.AvailableLives.ToString();
    }

    private void OnUpdateCoins(OnCoinsUpdateSignal signal)
    {
        gameData.Data.Coins += signal.Amount;
        PlayAnimation(coinsAnimText, coinsCanvasGroup, coinsText.transform.position, signal.Amount);
        SaveAndRefresh();
        ReceiveSpendCoins(signal.Amount);
    }

    void ReceiveSpendCoins(int rewardAmount)
    {
        if (rewardAmount < 0)
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.SpendCoins, Amount = Mathf.Abs(rewardAmount) });
    }

    private void OnUpdateLives(OnHealthUpdateSignal signal)
    {
        gameData.Data.AvailableLives += signal.TimeToAdd;
        PlayAnimation(livesAnimText, livesCanvasGroup, healthText.transform.position, signal.TimeToAdd);
        SaveAndRefresh();
    }

    private void OnUpdateHammer(OnHammerUpdateSignal signal)
    {
        // gameData.Data.Hammer += signal.Amount;
        //PlayAnimation(livesAnimText, livesCanvasGroup, healthText.transform.position, signal.Amount);
        SaveAndRefresh();
    }
    private void OnUpdateWand(OnWandUpdateSignal signal)
    {
        gameData.Data.Wand += signal.Amount;
        //PlayAnimation(livesAnimText, livesCanvasGroup, healthText.transform.position, signal.Amount);
        SaveAndRefresh();
    }
    private void OnUpdateSlotPopper(OnSlotPopperUpdateSignal signal)
    {
        // gameData.Data.SlotPopper += signal.Amount;
        //PlayAnimation(livesAnimText, livesCanvasGroup, healthText.transform.position, signal.Amount);
        SaveAndRefresh();
    }
    private void OnUpdateMagnet(OnMagnetUpdateSignal signal)
    {
        gameData.Data.Magnets += signal.Amount;
        //PlayAnimation(livesAnimText, livesCanvasGroup, healthText.transform.position, signal.Amount);
        SaveAndRefresh();
    }
    private void PlayAnimation(
        TextMeshProUGUI animText,
        CanvasGroup canvasGroup,
        Vector3 startPosition,
        int amount)
    {
        if (amount == 0) return;
        animText.transform.position = startPosition;
        animText.color = amount > 0 ? Color.green : Color.red;
        animText.text = amount > 0 ? $"+{amount}" : amount.ToString();

        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0f, 2f);

        animText.rectTransform.anchoredPosition = Vector2.zero;
        animText.rectTransform.DOAnchorPos(Vector2.down * 50f, 2f);
    }

    private void SaveAndRefresh()
    {
        gameData.Save();
        UpdateUI();
    }
}
