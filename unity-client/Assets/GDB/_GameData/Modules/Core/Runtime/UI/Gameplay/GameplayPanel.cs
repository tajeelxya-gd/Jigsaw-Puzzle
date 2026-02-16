using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPanel : MonoBehaviour
{
    [SerializeField] private RectTransform _middleTopHeaderRect;
    [SerializeField] private PowerUpButtonController _powerUpButtonController;
    [SerializeField] private PowerUpInfoPanel _powerUpInfoPanel;
    [SerializeField] private LevelBar _levelBar;
    [SerializeField] private ProgressBarBase _levelProgressBar;
    [SerializeField] private CollectableVisual _enemyCollectableVisual;
    [SerializeField] private PiggyCollectableUI _piggyCollectableVisual;
    [SerializeField] private PiggyCollectableAnimationControl _piggyCollectableAnimationControl;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private CoinBar _coinBar;

    public IProgress LevelProgressVisual => _levelProgressBar;
    private SettingsManager _settingsManager;
    public void Initialize(LevelData levelData, SettingsManager settingsManager, CannonController cannonController, SpaceController spaceController)
    {
        _settingsManager = settingsManager;

        _powerUpButtonController.Initialize(spaceController, cannonController);
        _powerUpInfoPanel.Initialize(_powerUpButtonController);
        _levelBar.Initialize(levelData.levelType);
        _levelProgressBar.Initialize();
        _enemyCollectableVisual.Initialize();
        _coinBar.Initialize();

        _settingsButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.AddListener(() =>
        {
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
            _settingsManager.OpenSettingPanel();
        });

        _piggyCollectableVisual.gameObject.SetActive(levelData.HasGoal);
        if (levelData.HasGoal)
        {
            _middleTopHeaderRect.gameObject.SetActive(false);
            _piggyCollectableAnimationControl.Animate(levelData.GoalAmount, () =>
            {
                _middleTopHeaderRect.gameObject.SetActive(true);
                _middleTopHeaderRect.DOScale(Vector3.one, 0.5f).From(Vector3.one * 1.2f).SetEase(Ease.OutCubic);
            });
            _piggyCollectableVisual.Initialize(levelData.GoalAmount);
        }
    }
}