using System;
using System.Collections;
using System.Collections.Generic;
using Client.Runtime;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _GameData.Modules.Core.Runtime.UI.Screens.WinScreen
{
    public class FailScreen : ScreenBase
    {
        // -----------------------------
        //  ROOTS TAB
        // -----------------------------
        [TabGroup("FailScreen", "Roots")]
        [SerializeField]
        private CanvasGroup _root_CG;
        [TabGroup("FailScreen", "Roots")]
        [SerializeField]
        private GameObject _tryAgainRoot;
        [TabGroup("FailScreen", "Roots")]
        [SerializeField]
        private RectTransform _defeatContentTransform;
        [TabGroup("FailScreen", "Roots")]
        [SerializeField]
        private CanvasGroup _upperBundleRoot;
        [TabGroup("FailScreen", "Roots")]
        [SerializeField]
        private TMP_Text _levelNumber;

        // -----------------------------
        //  FAILED TAB
        // -----------------------------

        [TabGroup("FailScreen", "MainRoot")]
        [SerializeField]
        private Image _rootBG;
        [TabGroup("FailScreen", "MainRoot")]
        [SerializeField]
        private Button _crossButton;
        [TabGroup("FailScreen", "MainRoot")]
        [SerializeField]
        private Button _playOn;
        [TabGroup("FailScreen", "MainRoot")]
        [SerializeField]
        private int _playOnAmount;

        // -----------------------------
        //  PROGRESSION TAB
        // -----------------------------
        [TabGroup("FailScreen", "Progress Info")]
        [ReadOnly, SerializeField]
        private int _currentPlayedLevel = 0;

        [TabGroup("FailScreen", "Animation")]
        [SerializeField]
        private Image _slots;
        [TabGroup("FailScreen", "Animation")]
        [SerializeField]
        private Sprite _normalSlot;
        [TabGroup("FailScreen", "Animation")]
        [SerializeField]
        private Sprite _redSlot;
        [TabGroup("FailScreen", "Animation")]
        [SerializeField]
        private GameObject _bigHeartBlinkEffect;

        [TabGroup("FailScreen", "StoreBundles")]
        [SerializeField]
        private GameObject[] _bundles;

        [TabGroup("FailScreen", "InfinitePanel")]
        [SerializeField]
        private TextMeshProUGUI _infiniteTimer;
        [TabGroup("FailScreen", "InfinitePanel")]
        [SerializeField]
        private GameObject _infinitePanel;
        [TabGroup("FailScreen", "InfinitePanel")]
        [SerializeField]
        private Button _infiniteRetryButton;
        [TabGroup("FailScreen", "InfinitePanel")]
        [SerializeField]
        private Button _infiniteCloseButton;

        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private GameObject _levelFailPanel;
        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private Image[] _heartsSprites;

        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private Image[] _heartsSpritesRoot;

        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField] private TMP_Text _resetHealthTimerTmp;

        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private Sprite _breakableHeart;
        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private Sprite _heart;
        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private Button _retryButton;
        [TabGroup("FailScreen", "LevelFailPanel")]
        [SerializeField]
        private Button _failCloseButton;

        [TabGroup("FailScreen", "CannonFail")]
        [SerializeField]
        private GameObject _cannonPanel;

        [TabGroup("FailScreen", "WallBreakAnimation")]
        [SerializeField]
        private GameObject _wallBreakPanel;
        [TabGroup("FailScreen", "WallBreakAnimation")]
        [SerializeField]
        private Image _wallBreakImage;
        [TabGroup("FailScreen", "WallBreakAnimation")]
        [SerializeField]
        private Sprite[] _wallBreakAnimations;

        private bool _isInfinite = false;
        private List<GameObject> _instantiatedBundles = new List<GameObject>();
        ITimeService timeService_freeTimer;
        ITimeService timeService_resetHealthTimer;
        private Sequence _sequence;
        private LevelFailType _levelFailType;
        private Vector3 _initialTrophyPosition;
        private IPuzzleService _puzzleService;

        [SerializeField] private RectTransform _brokenHeartRoot;
        [SerializeField] private RectTransform _remainingHeartRoot;
        [SerializeField] private RectTransform _timeLabelRoot;
        [SerializeField] private RectTransform _timerAnimRoot;
        [SerializeField] private RectTransform _buttonsRoot;

        private bool _hasFailPanelShown;
        private bool _lifeSubtracted;

        public override void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            UniEvents.Subscribe<LevelStartEvent>(OnLevelLoaded);
        }

        private void OnDestory()
        {
            UniEvents.Unsubscribe<LevelStartEvent>(OnLevelLoaded);
        }

        private void OnEnable()
        {
            SignalBus.Subscribe<CloseFailPanelSignal>(CloseFailPanel);
            SignalBus.Subscribe<SlotsFullSignal>(SlotChecker);
            SignalBus.Subscribe<OnHealthUpdateSignal>(OnHealthUpdate);
            SignalBus.Subscribe<OnFailedSignal>(CallMainFailSignal);
            SignalBus.Subscribe<ChangeCannonSlotSignal>(ChangeCannonSprite);
            SignalBus.Subscribe<OnInAppBuySignal>(OnInAppBuySignalReceived);
        }

        private void Start()
        {
            PlayHeartBeatEffect();

            RegisterButtonListeners();
            LoadAndUpdateHealthTimerData();
            _instantiatedBundles.Clear();
            foreach (var bundle in _bundles)
            {
                _instantiatedBundles.Add(bundle);
            }
            RemoveHearts(_heartsSprites);
            RemoveHearts(_heartsSpritesRoot);
        }


        void LoadAndUpdateHealthTimerData()
        {
            timeService_freeTimer =
                new RealTimeService(PlayerHealthTimerType.InfiniteHealthTimer.ToString(), OnInfiniteHealth);
            timeService_resetHealthTimer =
                new RealTimeService(PlayerHealthTimerType.ResetHealthTimer.ToString(), OnNormalHeath);
            _isInfinite = timeService_freeTimer.IsRunning();
        }

        void OnHealthUpdate(OnHealthUpdateSignal signal)
        {
            timeService_resetHealthTimer.ExtendTimer(signal.TimeToAdd);
        }

        // private void AssignLevelFailType(OnlevelFailSignal signal)
        // {
        //     Debug.LogError(signal.levelFailType);
        //     _levelFailType = signal.levelFailType;
        // }
        void OnLevelLoaded(LevelStartEvent signal)
        {
            _currentPlayedLevel = signal.LevelIndex + 1;
            _levelNumber.SetText($"LEVEL {_currentPlayedLevel}");
        }

        private void OnInAppBuySignalReceived(OnInAppBuySignal signal)
        {
            OnIapPurchased();
        }

        void OnIapPurchased()
        {
            // ResumeGame();
            // ResetAll();
            // // SignalBus.Publish(new OnRevivalSignal());
            // // SignalBus.Publish(new OnWallRevivalSignal());
            // SignalBus.Publish(new InputRestrictSignal { restrict = false });
        }
        private void Update()
        {
            timeService_freeTimer.Update();
            timeService_resetHealthTimer.Update();

            if (_isInfinite)
            {
                _infiniteTimer.text = timeService_freeTimer.GetFormattedTimeMinutes();
            }
            if (timeService_resetHealthTimer.IsRunning())
            {
                _resetHealthTimerTmp.text = timeService_resetHealthTimer.GetFormattedTime();
            }
            else
            {
                _resetHealthTimerTmp.text = "25M";
            }
        }

        #region BUTTONS
        void RegisterButtonListeners()
        {
            _infiniteRetryButton.onClick.AddListener(OnClick_InfiniteContinueButton);
            _playOn.onClick.AddListener(OnClick_PlayOnForCoins);
            _infiniteCloseButton.onClick.AddListener(() =>
            {
                GoToHome();
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
              "Level " + _currentPlayedLevel,
             "Fail",
             "ScreenInfinite",
             "Cross");
            });
            _failCloseButton.onClick.AddListener(() =>
            {
                GoToHome();
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
               "Level " + _currentPlayedLevel,
              "Fail",
              "ScreenRetry",
              "Cross");
            });
            _retryButton.onClick.AddListener(OnClick_TryAgain);
        }

        void ContinueButton()
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                HideScreen();
                PoolManager.ReturnAllItems();
                SignalBus.Publish(new ONGameResetSignal());
                _puzzleService.RestartPuzzleAsync(false, this.GetCancellationTokenOnDestroy());
            });
        }

        void OnClick_InfiniteContinueButton()
        {
            //ContinueButton();
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
            _infinitePanel.transform.GetChild(0).DOScale(0.2f, 0.25f);
            SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = true, FakeLoadTime = 3 });
            DOVirtual.DelayedCall(0.1f, ResetAll);
            ContinueButton();
            GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
           "Level " + _currentPlayedLevel,
          "Fail",
          "ScreenInfinite",
          "Retry");
        }

        void ContinueWithoutDelay()
        {
            HideScreen();
        }

        void OnClick_TryAgain()
        {
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
            if (GlobalService.GameData.Data.AvailableLives > 0)
            {
                DOVirtual.DelayedCall(0.1f, ResetAll);
                _levelFailPanel.transform.GetChild(0).DOScale(0.2f, 0.25f);
                ContinueButton();
                // SignalBus.Publish(new OnSceneShiftSignal { SceneName = "GamePlay", DoFakeLoad = true, FakeLoadTime = 3 });
                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
                 "Level " + _currentPlayedLevel,
                "Fail",
                "ScreenRetry",
                "Retry");
                //ContinueButton();
            }
            else
            {
                SignalBus.Publish(new OnNoMoreLivesSignal());
            }


            RemoveHearts(_heartsSprites);
            RemoveHearts(_heartsSpritesRoot);
            //Implement Continue Button
        }

        void OnClick_InfiniteButton()
        {
            ResetAll();
            _infinitePanel.SetActive(true);
            PlayHeartBeatEffect();
            GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
             "Level " + _currentPlayedLevel,
            "Fail",
            "ScreenInfinite",
            "Show");
        }
        void PlayHeartBeatEffect()
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(_bigHeartBlinkEffect.transform.DOScale(0.95f, 0.15f))
                .Append(_bigHeartBlinkEffect.transform.DOScale(1f, 0.15f))
                // .Append(_bigHeartBlinkEffect.transform.DOScale(0.9f, 0.1f))
                // .Append(_bigHeartBlinkEffect.transform.DOScale(1f, 0.1f))
                .AppendInterval(0.5f)
                .SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }

        private int _plaYOnCounter = 0;
        void OnClick_PlayOnForCoins()
        {
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
            _plaYOnCounter++;
            int _totalUserCoins = GlobalService.GameData.Data.Coins;
            if (_totalUserCoins >= _playOnAmount && _levelFailType == LevelFailType.OutOFSpace)
            {
                GlobalService.GameData.Data.Coins -= _playOnAmount;
                GlobalService.GameData.Save();
                ResumeGame();
                SignalBus.Publish(new AddCoinsSignal { Amount = -_playOnAmount, IsAdd = false });
                ResetAll();
                SignalBus.Publish(new OnRevivalSignal());
                SignalBus.Publish(new OnWallRevivalSignal());
                SignalBus.Publish(new InputRestrictSignal { restrict = false });

                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
                 "Level " + _currentPlayedLevel,
                "Fail",
                "Screen1",
                "Revive");
            }
            else if (_totalUserCoins >= _playOnAmount && _levelFailType == LevelFailType.WallBreak)
            {
                GlobalService.GameData.Data.Coins -= _playOnAmount;
                GlobalService.GameData.Save();
                ResumeGame();
                SignalBus.Publish(new AddCoinsSignal { Amount = -_playOnAmount, IsAdd = false });
                ResetAll();
                SignalBus.Publish(new OnWallRevivalSignal());
                SignalBus.Publish(new InputRestrictSignal { restrict = false });

                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
                 "Level " + _currentPlayedLevel,
                "Fail",
                "Screen1",
                "Revive");
            }
            else if (_totalUserCoins >= _playOnAmount)
            {
                GlobalService.GameData.Data.Coins -= _playOnAmount;
                GlobalService.GameData.Save();
                SignalBus.Publish(new AddCoinsSignal { Amount = -_playOnAmount, IsAdd = false });
                HideScreen();
                ResetAll();
                _puzzleService.RestartPuzzleAsync(true, this.GetCancellationTokenOnDestroy());

                GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
               "Level " + _currentPlayedLevel,
              "Fail",
              "Screen1",
              "Revive");
            }
            else
            {

                HideScreen();
                SignalBus.Publish(new OnCoinBundleCalledSignal
                {
                    OnClose = () =>
                    {
                        ResetAll();
                        ShowScreen(_levelFailType);
                    }
                });
            }
        }
        #endregion

        #region ANIMATIONS
        private void PlayHeartAnimation(Image[] heartsSprites)
        {
            int availableLives = GlobalService.GameData.Data.AvailableLives;
            int blinkIndex = availableLives - 1;
            if (blinkIndex < 0 || blinkIndex >= heartsSprites.Length) return;

            Image heart = heartsSprites[blinkIndex];
            heart.DOKill();
            heart.DOFade(0.2f, 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine).SetUpdate(true);
        }



        private void RemoveHearts(Image[] heartsSprites)
        {
            int availableLives = GlobalService.GameData.Data.AvailableLives;
            for (int i = 0; i < heartsSprites.Length; i++)
            {
                heartsSprites[i].DOKill();
                if (i < availableLives)
                {
                    heartsSprites[i].gameObject.SetActive(true);
                    heartsSprites[i].color = Color.white;
                    heartsSprites[i].sprite = _heart;
                }
                else
                {
                    heartsSprites[i].gameObject.SetActive(false);
                }
            }
        }

        void PlayDefeatAnimation()
        {
            _defeatContentTransform.gameObject.SetActive(true);
            _rootBG.DOFade(1, 0.75f).From(0).SetUpdate(true);
        }

        private Coroutine _wallBreakRoutine;

        private void PlayWallBreakAnimation()
        {
            if (_wallBreakRoutine == null)
                _wallBreakRoutine = StartCoroutine(WallBreakAnimation());
        }

        private IEnumerator WallBreakAnimation()
        {
            yield return new WaitForSecondsRealtime(1f);

            while (true)
            {
                for (int i = 0; i < _wallBreakAnimations.Length; i++)
                {
                    _wallBreakImage.sprite = _wallBreakAnimations[i];
                    yield return new WaitForSecondsRealtime(0.05f);
                }
            }
        }


        private void ChangeCannonSprite(ChangeCannonSlotSignal signal)
        {
            if (signal._isRed)
            {
                _slots.sprite = _redSlot;
            }
            else
            {
                _slots.sprite = _normalSlot;
            }
        }

        #endregion

        #region Main

        public override void HideScreen()
        {
            base.HideScreen();
            _root_CG.DOFade(0, 0.3f).From(1).OnComplete(() => { _root_CG.gameObject.SetActive(false); }).SetUpdate(true);
            SignalBus.Publish(new InputRestrictSignal { restrict = false });
            ResumeGame();
        }

        private bool _hasCannonSlotFilled = false;

        private void SlotChecker(SlotsFullSignal signal)
        {
            _hasCannonSlotFilled = signal.isSlotsFull;
        }
        [Button("Show Panel")]
        public override void ShowScreen<T>(T data)
        {
            if (data is LevelFailType levelFailTypeType)
                _levelFailType = levelFailTypeType;
            LoadAndUpdateHealthTimerData();
            SignalBus.Publish(new OnSpacesFullSignal());
            //Debug.LogError("Play on Counter" + _plaYOnCounter);
            if (_plaYOnCounter > 2)
            {
                PlayOnButtonCheck();
                _plaYOnCounter = 0;
                return;
            }
            // if (_hasFailPanelShown) return;
            Debug.LogError("Screen Shown");
            base.ShowScreen();
            ResetAll();
            PlayFailSequence();
            SignalBus.Publish(new InputRestrictSignal { restrict = true });
            _hasFailPanelShown = true;
            AudioController.PlaySFX(AudioType.Loss);
            HapticController.Vibrate(HapticType.LevelFail);
        }

        private void PlayFailSequence()
        {
            GlobalService.GameData.Data.CurrentWinStreakLevel = 0;
            GlobalService.GameData.Save();
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal
            { MissionType = MissionType.WinStreak, Amount = -100 });

            if (_isInfinite)
            {
                _upperBundleRoot.gameObject.SetActive(false);
                //InstantiateBundle(_tryAgainPanel.gameObject);
                _crossButton.onClick.RemoveAllListeners();
                OnClick_InfiniteButton();
                return;
            }

            _sequence = DOTween.Sequence();
            _sequence.SetUpdate(true);

            _sequence
                // ROOT FADE IN
                .AppendInterval(0.25f)
                .AppendCallback(() => _root_CG.gameObject.SetActive(true))
                .Append(_root_CG.DOFade(1, 0.3f))
                .AppendCallback(() => _upperBundleRoot.gameObject.SetActive(true))
                .Append(_upperBundleRoot.DOFade(1, 0.3f))

                // HEADER / DEFEAT TITLE
                .AppendCallback(PlayDefeatAnimation)
                .AppendInterval(0.5f)

                // 1. BROKEN HEART
                .AppendCallback(() => ShowSequenceElement(_brokenHeartRoot))
                .AppendInterval(0.05f)

                // 2. REMAINING HEART
                .AppendCallback(() =>
                {
                    ShowSequenceElement(_remainingHeartRoot);
                    RemoveHearts(_heartsSpritesRoot);
                    PlayHeartAnimation(_heartsSpritesRoot);
                })
                .AppendInterval(0.05f)

                // 3. TIME LABEL
                .AppendCallback(() => ShowSequenceElement(_timeLabelRoot))
                .AppendInterval(0.05f)

                // 4. TIMER ANIM
                .AppendCallback(() => ShowSequenceElement(_timerAnimRoot))
                .AppendInterval(0.05f)

                // 5. BUTTONS (delayed for emphasis)
                .AppendInterval(0.8f)
                .AppendCallback(() => ShowSequenceElement(_buttonsRoot));

            Debug.LogError(_levelFailType);
            if (_levelFailType == LevelFailType.OutOFSpace)
            {
                _cannonPanel.SetActive(true);
            }
            else if (_levelFailType == LevelFailType.WallBreak)
            {
                _wallBreakPanel.SetActive(true);
                PlayWallBreakAnimation();
            }

            if (_isInfinite)
            {
                _upperBundleRoot.gameObject.SetActive(false);
                //InstantiateBundle(_tryAgainPanel.gameObject);
                _crossButton.onClick.RemoveAllListeners();
                _crossButton.onClick.AddListener(() =>
                {
                    OnClick_InfiniteButton();
                });
            }
            else if (GlobalService.GameData.Data.AvailableLives == 0)
            {
                _crossButton.onClick.RemoveAllListeners();
                _crossButton.onClick.AddListener(() =>
                {
                    ShowNoMoreLivesPanel();
                });
            }
            else
            {
                _crossButton.onClick.RemoveAllListeners();
                _crossButton.onClick.AddListener(() =>
                {
                    OpenLevelFailedPanel();
                });
            }
            _crossButton.onClick.AddListener(() => GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
                 "Level " + _currentPlayedLevel,
                "Fail",
                "Screen1",
                "Cross"));
            //InstantiateBundle(_upperBundleRoot.gameObject);
            GlobalService.GameData.Data.BackFromWin = false;
            GlobalService.GameData.Save();
        }

        private void ShowSequenceElement(RectTransform element)
        {
            if (element == null) return;
            element.gameObject.SetActive(true);
            element.localScale = Vector3.one;

            // Move from bottom effect
            float offset = 100f;
            Vector2 targetPos = element.anchoredPosition;
            element.anchoredPosition = new Vector2(targetPos.x, targetPos.y - offset);

            element.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);

            // Subtle fade in
            CanvasGroup cg = element.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0;
                cg.DOFade(1f, 0.3f).SetUpdate(true);
            }
        }

        private void PlayOnButtonCheck()
        {
            if (_isInfinite)
            {
                OnClick_InfiniteButton();
            }
            else if (GlobalService.GameData.Data.AvailableLives == 0)
            {
                ShowNoMoreLivesPanel();
            }
            else
            {
                OpenLevelFailedPanel();
            }
        }
        #endregion

        #region Panels

        void OpenLevelFailedPanel()
        {
            _levelFailPanel.SetActive(true);
            RemoveHearts(_heartsSprites);
            RemoveHearts(_heartsSpritesRoot);
            PlayHeartAnimation(_heartsSprites);
            PlayHeartAnimation(_heartsSpritesRoot);

            if (!_lifeSubtracted && !_isInfinite)
            {
                GlobalService.GameData.Data.AvailableLives = Math.Max(0, GlobalService.GameData.Data.AvailableLives - 1);
                GlobalService.GameData.Save();
                _lifeSubtracted = true;
            }

            GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
                    "Level " + _currentPlayedLevel,
                      "Fail",
                      "ScreenRetry",
                      "Show");
        }

        void ShowNoMoreLivesPanel()
        {
            ResetAll();
            SignalBus.Publish(new OnNoMoreLivesSignal());
            GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression",
                      "Level " + _currentPlayedLevel,
                      "Fail",
                      "ScreenNoMoreLives",
                      "Show");
        }

        private void ShowTryAgainPanel()
        {
            // This is now handled in the PlayFailSequence via _remainingHeartRoot
            // _tryAgainRoot.gameObject.SetActive(true);
            // _tryAgainRoot.transform.localScale = Vector3.zero;
            // _tryAgainRoot.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

            // RemoveHearts(_heartsSpritesRoot);
            // PlayHeartAnimation(_heartsSpritesRoot);
        }

        private void CloseFailPanel(CloseFailPanelSignal signal)
        {
            DOVirtual.DelayedCall(0.1f, ResetAll);
            SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = true, FakeLoadTime = 4, OnFakeLoadCompleteEven = ContinueButton });
            //ContinueButton();
        }


        private void GoToHome()
        {
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);

            if (!_lifeSubtracted && !_isInfinite)
            {
                GlobalService.GameData.Data.AvailableLives = Math.Max(0, GlobalService.GameData.Data.AvailableLives - 1);
                GlobalService.GameData.Save();
                _lifeSubtracted = true;
            }

            SignalBus.Publish(new OnSceneShiftSignal { DoFakeLoad = true, FakeLoadTime = 2, OnFakeLoadCompleteEven = LoadMainMenu });
        }

        private void LoadMainMenu()
        {
            SceneManager.LoadScene(1);
        }
        #endregion


        private void ResetAll()
        {
            if (_wallBreakRoutine != null)
            {
                StopCoroutine(_wallBreakRoutine);
                _wallBreakRoutine = null;
            }
            _instantiatedBundles = new List<GameObject>(_bundles);
            _slots.sprite = _normalSlot;
            _root_CG.gameObject.SetActive(false);

            // Reset scales for animated panels
            _tryAgainRoot.transform.localScale = Vector3.one;
            _infinitePanel.transform.GetChild(0).localScale = Vector3.one;
            _levelFailPanel.transform.GetChild(0).localScale = Vector3.one;

            _tryAgainRoot.gameObject.SetActive(false);
            _defeatContentTransform.gameObject.SetActive(false);
            _brokenHeartRoot.gameObject.SetActive(false);
            _remainingHeartRoot.gameObject.SetActive(false);
            _timeLabelRoot.gameObject.SetActive(false);
            _timerAnimRoot.gameObject.SetActive(false);
            _buttonsRoot.gameObject.SetActive(false);

            _cannonPanel.SetActive(false);
            _wallBreakPanel.SetActive(false);
            _infinitePanel.SetActive(false);
            _upperBundleRoot.gameObject.SetActive(true);
            _levelFailPanel.SetActive(false);
            _hasFailPanelShown = false;
            _hasCannonSlotFilled = false;
            _lifeSubtracted = false;
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
        }


        void OnInfiniteHealth()
        {
        }

        void OnNormalHeath()
        {
        }

        private void CallMainFailSignal(OnFailedSignal signal)
        {
            _levelFailPanel.SetActive(true);
            RemoveHearts(_heartsSprites);
            RemoveHearts(_heartsSpritesRoot);
            PlayHeartAnimation(_heartsSprites);
            PlayHeartAnimation(_heartsSpritesRoot);
        }

        private void OnDisable()
        {
            SignalBus.Unsubscribe<CloseFailPanelSignal>(CloseFailPanel);
            SignalBus.Unsubscribe<SlotsFullSignal>(SlotChecker);
            SignalBus.Unsubscribe<OnFailedSignal>(CallMainFailSignal);
            SignalBus.Unsubscribe<ChangeCannonSlotSignal>(ChangeCannonSprite);
            SignalBus.Unsubscribe<OnInAppBuySignal>(OnInAppBuySignalReceived);
            SignalBus.Unsubscribe<OnHealthUpdateSignal>(OnHealthUpdate);
            _sequence?.Kill();
        }

        private void OnApplicationQuit()
        {
            if (_isInfinite || _lifeSubtracted) return;
            GlobalService.GameData.Data.AvailableLives = Math.Max(0, GlobalService.GameData.Data.AvailableLives - 1);
            GlobalService.GameData.Save();
            _lifeSubtracted = true;
        }

        private void SendLevelFailAnalytics()
        {
            int level = GlobalService.GameData.Data.LevelIndex;
            GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Progression", "Level " + level, "Fail", "Screen1", "Show");
        }
    }
}

public class OnFailedSignal : ISignal { }