using System;
using System.Collections;
using System.Threading;
using Client.Runtime;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _GameData.Modules.Core.Runtime.UI.Screens.WinScreen
{
    public class WinScreen : ScreenBase
    {
        // -----------------------------
        //  DATA TAB
        // -----------------------------
        [TabGroup("WinScreen", "Data")]
        [SerializeField]
        private ProceduralAchievementsData _proceduralAchievementsData;


        // -----------------------------
        //  ROOTS TAB
        // -----------------------------
        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private CanvasGroup _root_CG;

        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private CanvasGroup _victory_CG;

        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private GameObject _buttonsRoot;

        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private CanvasGroup _achievementFiller_CG;

        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private CanvasGroup _economyRootTransform_CG;

        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private CanvasGroup _completionFlash;

        [TabGroup("WinScreen", "Roots")]
        [SerializeField] private CanvasGroup _completionFlash_glow;


        // -----------------------------
        //  VICTORY TAB
        // -----------------------------
        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private RectTransform _victoryTransform;

        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private Vector2 _defaultvictoryPosition;

        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private Vector2 _animatedvictoryPosition;

        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private TextMeshProUGUI _currentLevelTxt;

        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private StreakModelViewController _streakModelViewController;
        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private GameObject _yourRewardObj;

        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private GameObject _trophy;
        [TabGroup("WinScreen", "Victory")]
        [SerializeField] private Transform[] _trophyWaypoints;
        [SerializeField] private Image _levelCompleteImage;


        // -----------------------------
        //  ACHIEVEMENT FILLER TAB
        // -----------------------------
        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private Image _achievementFiller_Bg;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private GameObject _unlockImg;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private GameObject _toLockImg;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private GameObject[] _fillerEffects;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private Image _achievementFiller;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private RectTransform _achievementFillerTransform;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private Vector2 _defaultAchievementFiller_Position;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private Vector2 _animatedAchievementFiller_Position;

        [TabGroup("WinScreen", "Achievement Filler")]
        [SerializeField] private TextMeshProUGUI _fillPercentage;



        // -----------------------------
        //  ECONOMY TAB
        // -----------------------------
        [TabGroup("WinScreen", "Economy")]
        [SerializeField] private RectTransform _economyRootTransform;

        [TabGroup("WinScreen", "Economy")]
        [SerializeField] private TextMeshProUGUI _totalRewardText, _totalEnemyCurrencyRewardTxt, _totalTrophyCount;
        [TabGroup("WinScreen", "Economy")]
        [SerializeField] private GameObject _coinsRewardObject, _enemyCurrencyRewardObject, _trophyRewardObject;


        // -----------------------------
        //  FX & BUTTONS TAB
        // -----------------------------
        [TabGroup("WinScreen", "FX & Buttons")]
        [SerializeField] private ParticleSystem _particleSystem_Win;

        [TabGroup("WinScreen", "FX & Buttons")]
        [SerializeField] private Button _continueButton, _doubleRewardButton; //_homeButton;


        [TabGroup("WinScreen", "FX & Buttons")]
        [SerializeField]
        private RectTransform _buttonsContainer;
        // -----------------------------
        //  PROGRESSION TAB
        // -----------------------------
        [TabGroup("WinScreen", "Progress Info")]
        [ReadOnly, SerializeField]
        private int _currentPlayedLevel = 0;

        private int _currentEnemyCurrency = 0;

        [TabGroup("WinScreen", "OnBoarding")]
        [SerializeField]
        private OnBoardingConfig.OnBoardingType[] onBoardingLevels;
        // -----------------------------
        //  ON LEVEL COMPLETE TAKE USER BACK TO GAMEPLAY
        // -----------------------------


        // -----------------------------
        //  UNITY EVENTS
        // -----------------------------

        private void Start()
        {
            UniEvents.Subscribe<LevelStartEvent>(OnLevelLoaded);
            RegisterButtonListeners();
        }

        void RegisterButtonListeners()
        {
            _continueButton.onClick.AddListener(OnClick_DelayedContinueButton);
            _doubleRewardButton.onClick.AddListener(OnClick_DoubleRewardButton);
        }

        void OnClick_HomeButton()
        {
            //Implement Home Functionality Here
            SceneManager.LoadScene(0);
        }

        void OnClick_DelayedContinueButton()
        {
            _doubleRewardButton.enabled = false;
            _continueButton.enabled = false;
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.EarnCoins, Amount = levelData.CoinRewardAmount * 1 });
            SignalBus.Publish(new AddCoinsSignal { Amount = levelData.CoinRewardAmount * 1 });
            PunchHideButtons();
            DOVirtual.DelayedCall(2f, OnClick_ContinueButton);
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
        }

        void GoToMainMenu()
        {
            gameObject.SetActive(false);
            SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = false, SceneName = "MainMenu", levelType = _clearLevelType });
        }

        void ShowAd()
        {
            // if (_currentPlayedLevel > RemoteConfigManager.Configuration.ShowAdAfter)
            //     AdsManager.ShowInterstitial("LevelComplete " + _currentPlayedLevel);
        }

        void OnClick_ContinueButton()
        {
            DOVirtual.DelayedCall(0.75f, ShowAd);
            _doubleRewardButton.enabled = false;
            _continueButton.enabled = false;
            int upcomingLevel = GlobalService.GameData.Data.LevelIndex;
            if (upcomingLevel >= (int)OnBoardingConfig.OnBoardingType.WeeklyRewards)
            {
                GoToMainMenu();
                return;
            }

            // foreach (var onBoardingLevel in onBoardingLevels)
            // {
            //     if ((int)onBoardingLevel == upcomingLevel)
            //         if (!OnBoardingConfig.HasOnBoardingActivatedBefore(onBoardingLevel))
            //         {
            //             GoToMainMenu();
            //             return;
            //         }

            // }

            PoolManager.ReturnAllItems();
            HideScreen();
            void LevelLoaded() => SignalBus.Publish(new ONGameResetSignal());
            SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = true, FakeLoadTime = 2.5f, OnFakeLoadCompleteEven = LevelLoaded, levelType = _clearLevelType });

        }

        void OnClick_DoubleRewardButton()
        {
            //Implement Double Reward
            // AdsManager.ShowRewarded("DoubleReward " + _currentPlayedLevel, () =>
            // {
            PunchHideButtons();
            _doubleRewardButton.enabled = false;
            _continueButton.enabled = false;
            SignalBus.Publish(new AddCoinsSignal { Amount = GetTotalReward() });
            PoolManager.ReturnAllItems();
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.Get2XCoins, Amount = 1 });
            DOVirtual.DelayedCall(Time.deltaTime, () =>
            {
                SignalBus.Publish(new OnMissionObjectiveCompleteSignal
                { MissionType = MissionType.EarnCoins, Amount = GetTotalReward() });
            });
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
            DOVirtual.DelayedCall(2f, OnClick_ContinueButton);
            // });
        }

        void PunchHideButtons()
        {
            _totalRewardText.text = "0";
            _buttonsRoot.transform.DOScale(0, 0.5f).SetEase(Ease.InBack).SetDelay(0.1f);

        }
        int GetTotalReward()
        {
            return levelData.CoinRewardAmount;
            // return levelData.CoinRewardAmount * RemoteConfigManager.Configuration.CoinMultiplier;//return your reward from here
        }

        public void GrantRewardToPlayer()//Give Reward here
        {

        }

        void OnLevelLoaded(LevelStartEvent signal)
        {
            _currentPlayedLevel = signal.LevelIndex + 1;
            SetLevelCompleteImage();
        }

        private void SetLevelCompleteImage()
        {
            var resourceLoader = UniStatics.Resolver.Resolve<IJigsawResourceLoader>();
            _levelCompleteImage.sprite = resourceLoader.GetLoadedImage();
        }

        public override void HideScreen()
        {
            base.HideScreen();
            _root_CG.DOFade(0, 0.3f).From(1).OnComplete(() => { _root_CG.gameObject.SetActive(false); });
        }

        private LevelType _clearLevelType;

        [Button("Show Panel")]
        public override void ShowScreen<T>(T data)
        {
            if (data is LevelType levelType)
                _clearLevelType = levelType;

            base.ShowScreen();
            ResetAll();
            PlayWinSequence();
            SendLevelWinAnalytics();
        }
        // -----------------------------
        //  INTERNAL LOGIC
        // -----------------------------
        private void ResetAll()
        {
            _root_CG.gameObject.SetActive(false);
            _victory_CG.gameObject.SetActive(false);
            _unlockImg?.gameObject.SetActive(false);
            _toLockImg?.gameObject.SetActive(false);
            _particleSystem_Win.gameObject.SetActive(false);
            _economyRootTransform.gameObject.SetActive(false);
            _achievementFillerTransform.gameObject.SetActive(false);
            _completionFlash_glow.gameObject.SetActive(false);
            _completionFlash.gameObject.SetActive(false);
            _completionFlash.alpha = 1;
            _completionFlash.transform.localScale = Vector3.one;
            _continueButton.gameObject.SetActive(false);
            _doubleRewardButton.gameObject.SetActive(false);
            _totalRewardText.text = "0";
            _doubleRewardButton.transform.localScale = Vector3.one;
            _continueButton.transform.localScale = Vector3.one;
            _buttonsRoot.transform.localScale = Vector3.one;
            foreach (var fillerEffect in _fillerEffects)
                fillerEffect.gameObject.SetActive(false);
            _victoryTransform.anchoredPosition = _defaultvictoryPosition;
            _achievementFillerTransform.anchoredPosition = _defaultAchievementFiller_Position;
            _continueButton.transform.localScale = Vector3.one;
            _doubleRewardButton.enabled = true;
            _continueButton.enabled = true;
            _hasReported = false;
            _streakModelViewController.gameObject.SetActive(false);
            _yourRewardObj.gameObject.SetActive(false);
            _unlockImg.gameObject.SetActive(false);
        }
        private Sequence _sequence;

        private bool _hasReported = false;
        void ReportMissionObjectives()
        {
            if (_hasReported)
            {
                Debug.LogWarning("ReportMissionObjectives called again!");
                return;
            }

            _hasReported = true;
            Debug.LogError("Reporting mission objectives");
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.WinStreak, Amount = 1 });
        }

        private void PlayWinSequence()
        {
            Debug.LogError("Playing Win sequence");

            GameData _gameData = GlobalService.GameData;
            GetLevelData(_currentPlayedLevel);
            if (_gameData.Data.IsPuzzleManiaUnlocked)
            {
                _gameData.Data.CurrentLevelEnemies = _gameData.Data.TempCollectedEnemies;
                _gameData.Data.TempCollectedEnemies = 0;
                _gameData.Data.CurrentLevelEnemies += UnityEngine.Random.Range(100, 300);
                _currentEnemyCurrency = _gameData.Data.CurrentLevelEnemies;
            }
            _gameData.Data.Coins += GetTotalReward();

            int currentPlayedLevel = _gameData.Data.LevelIndex;
            if (!_gameData.Data.IsLeaderBoardUnlocked)
                if ((currentPlayedLevel + 1) >= (int)OnBoardingConfig.OnBoardingType.LeaderBoard)
                {
                    _gameData.Data.IsLeaderBoardUnlocked = true;
                    Debug.LogError("LeaderBoard Unlocked!!");
                }
            _yourRewardObj.SetActive(!_gameData.Data.IsLeaderBoardUnlocked);
            _trophyRewardObject.gameObject.SetActive(_gameData.Data.IsLeaderBoardUnlocked);
            _gameData.Data.BackFromWin = true;
            _gameData.Save();

            ReportMissionObjectives();
            var current = _proceduralAchievementsData.GetAchievementProgress(_currentPlayedLevel);
            var previous = _proceduralAchievementsData.GetAchievementProgress(Mathf.Max(1, _currentPlayedLevel - 1));

            _achievementFiller.fillAmount = _proceduralAchievementsData.IsStartLevel(_currentPlayedLevel)
                ? 0
                : previous.FillAmount;

            _achievementFiller.sprite = current.RewardSprite;
            _achievementFiller_Bg.sprite = current.RewardSprite;

            float targetFill = current.FillAmount;

            _sequence = DOTween.Sequence();

            _sequence
                // ROOT FADE IN
                .AppendCallback(() => _root_CG.gameObject.SetActive(true))
                .Append(_root_CG.DOFade(1, 0.3f))
                .AppendCallback(PlayWinVfx)

                // VICTORY FX
                .AppendInterval(0.2f)
                .AppendCallback(() => PlayVictoryAnimation())

                // ECONOMY FX
                .AppendInterval(0.4f)
                .AppendCallback(() => PlayEconomyAnimation())

                // ACHIEVEMENT
                .AppendInterval(0.4f)
                .AppendCallback(() => ShowAchievementBase(previous.FillAmount))
                .Append(_achievementFiller.DOFillAmount(targetFill, 1f))
                .JoinCallback(PlayFillerBgEffect)
                // .Join(UpdateFillPercentageTween(targetFill, previous.FillAmount))
                // COMPLETION FX
                .AppendCallback(() =>
                {
                    if (targetFill >= 1f)
                        PlayCompletionFlash(current.RewardSprite);

                })

                // BUTTONS
                .AppendInterval(0.5f)
                .AppendCallback(ShowButtons)

                // Reward
                .AppendCallback(GrantRewardToPlayer);

            AudioController.StopBG();
            AudioController.PlaySFX(AudioType.Win);
            HapticController.Vibrate(HapticType.Win);
        }

        void PlayWinVfx()
        {
            _particleSystem_Win.gameObject.SetActive(true);
            _particleSystem_Win.Play();
        }

        private void PlayVictoryAnimation()
        {
            _currentLevelTxt.text = "Level " + _currentPlayedLevel;
            _victory_CG.gameObject.SetActive(true);
            _victory_CG.alpha = 0;
            _victory_CG.DOFade(1, 0.3f);
            _victoryTransform.localScale = Vector3.zero;
            _victoryTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => _victoryTransform.DOAnchorPos(_animatedvictoryPosition, 0.5f).SetEase(Ease.InOutBack));
        }

        private void PlayFillerBgEffect()
        {
            foreach (var fillerEffect in _fillerEffects)
                fillerEffect.gameObject.SetActive(true);
        }

        private void PlayEconomyAnimation()
        {
            _totalRewardText.text = (levelData.CoinRewardAmount * 1).ToString();
            // _totalRewardText.text = (levelData.CoinRewardAmount * RemoteConfigManager.Configuration.CoinMultiplier).ToString();
            _totalEnemyCurrencyRewardTxt.text = _currentEnemyCurrency.ToString();

            _enemyCurrencyRewardObject.gameObject.SetActive(GlobalService.GameData.Data.IsPuzzleManiaUnlocked);
            _economyRootTransform.gameObject.SetActive(true);
            _economyRootTransform_CG.alpha = 0;
            _economyRootTransform.DOScale(1f, 0.3f).From(3f);
            _economyRootTransform_CG.DOFade(1, 0.3f);

            if (GlobalService.GameData.Data.IsLeaderBoardUnlocked)
                TrophyAnimation();

        }

        private void ShowAchievementBase(float previousFill)
        {
            // _achievementFillerTransform.gameObject.SetActive(true);
            // _achievementFiller_CG.alpha = 0;
            // _achievementFiller_CG.DOFade(1, 0.5f);
            // _achievementFillerTransform.DOAnchorPos(_animatedAchievementFiller_Position, 1f).SetEase(Ease.InOutSine);
            // _fillPercentage.text = Mathf.RoundToInt(previousFill * 100f) + "%";
            GameData _gameData = GlobalService.GameData;
            if (_gameData.Data.IsLeaderBoardUnlocked)
            {
                _gameData.Data.CurrentWinStreakLevel = Mathf.Clamp(_gameData.Data.CurrentWinStreakLevel, 0, 4);
                _streakModelViewController.gameObject.SetActive(true);
                _streakModelViewController.UpdateStreaks();
                _gameData.Data.TrophiesWinInGame = 3 * _streakModelViewController.GetCurrentStreakRewardMultiplier();
                _totalTrophyCount.text = _gameData.Data.TrophiesWinInGame.ToString();
                _gameData.Data.CurrentWinStreakLevel++;
                _gameData.Save();
            }
        }

        private Tween UpdateFillPercentageTween(float target, float previous)
        {
            _toLockImg.gameObject.SetActive(!(target >= 1f));
            return DOTween.To(() => previous, x =>
            {
                _fillPercentage.text = Mathf.RoundToInt(x * 100f) + "%";
            }, target, 1f).OnComplete(() =>
            {
                _unlockImg.gameObject.SetActive(target >= 1f);
                _unlockImg.transform.DOScale(1, 0.5f).From(0.5f).SetEase(Ease.OutBack);
                _achievementFiller_CG.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 1);
            });


        }

        private void PlayCompletionFlash(Sprite sprite)
        {
            _completionFlash_glow.gameObject.SetActive(true);
            _completionFlash.gameObject.SetActive(true);

            _completionFlash.GetComponent<Image>().sprite = sprite;

            _completionFlash_glow.DOFade(0, 0.5f);

            _completionFlash.DOFade(0, 0.5f);
            _completionFlash.transform.DOScale(1.5f, 0.5f).SetEase(Ease.InOutSine);
            AudioController.PlaySFX(AudioType.PuzzleUnlock);
            HapticController.Vibrate(HapticType.Puzzle);
        }

        private Tween _rewardBtnTween;
        private void ShowButtons()
        {
            _rewardBtnTween?.Kill();

            _continueButton.transform.localScale = Vector3.zero;
            _continueButton.gameObject.SetActive(true);
            _continueButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            _doubleRewardButton.transform.localScale = Vector3.zero;
            _doubleRewardButton.gameObject.SetActive(true);
            _doubleRewardButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _rewardBtnTween = _doubleRewardButton.transform.DOScale(Vector3.one * 1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            });
        }

        private Vector3 _initialTrophyPosition;
        private Sequence _trophySeq;
        private void TrophyAnimation()
        {
            int winStreak = GlobalService.GameData.Data.CurrentWinStreakLevel;

            winStreak = Mathf.Clamp(winStreak, 0, _trophyWaypoints.Length - 1);

            _trophySeq?.Kill();

            _trophy.transform.position = _initialTrophyPosition;

            if (winStreak == 0)
                return;

            _trophySeq = DOTween.Sequence();

            for (int i = 1; i <= winStreak; i++)
            {
                int index = i;

                _trophySeq.Append(
                    _trophy.transform.DOJump(
                        _trophyWaypoints[index].position,
                        0.25f,
                        1,
                        0.6f
                    ).SetEase(Ease.OutQuad)
                );
            }
        }

        private int _totalLevelCount;
        private LevelData levelData;

        private void GetLevelData(int level)
        {
            _totalLevelCount = Resources.LoadAll<TextAsset>("Levels").Length;
            TextAsset json = Resources.Load<TextAsset>("Levels/Level " + Mathf.Clamp(level, 1, _totalLevelCount));
            LevelData temp = ScriptableObject.CreateInstance<LevelData>();
            JsonUtility.FromJsonOverwrite(json.text, temp);
            levelData = temp;
        }

        private int GetEnemyCount()
        {
            int enemyCount = 0;
            foreach (var enemy in levelData.enemyData)
            {
                enemyCount += enemy.enemyColumns.Length;
            }
            return enemyCount;
        }

        private void SendLevelWinAnalytics()
        {
            int level = _currentPlayedLevel;
            GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + level, "Completed", "CashLeft", GlobalService.GameData.Data.Coins.ToString());
            if (GameAnalytics.MagnetUsed > 0)
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + level, "Boosters", "Magnet", GameAnalytics.MagnetUsed.ToString());
            if (GameAnalytics.HammerUsed > 0)
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + level, "Boosters", "Hammer", GameAnalytics.HammerUsed.ToString());
            if (GameAnalytics.ShufflerUsed > 0)
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + level, "Boosters", "Shuffler", GameAnalytics.ShufflerUsed.ToString());
            if (GameAnalytics.SlotPopperUsed > 0)
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + level, "Boosters", "SlotPopper", GameAnalytics.SlotPopperUsed.ToString());
            GameAnalytics.ResetBoosterData();
        }

        //Fahad Code End

        private void OnDisable()
        {
            _sequence?.Kill();
            _trophySeq?.Kill();
            UniEvents.Unsubscribe<LevelStartEvent>(OnLevelLoaded);
        }
    }
}
