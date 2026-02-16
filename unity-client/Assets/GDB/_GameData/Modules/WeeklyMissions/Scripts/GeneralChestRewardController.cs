using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class GeneralChestRewardController : MonoBehaviour
{
    [SerializeField] private CrassModelViewConfiguration []_cratesModelConfigurationUI;
    [SerializeField] private Image[] _backFlares;
    [SerializeField] private Image _mainCrateImage;
    [SerializeField] private CanvasGroup _crateFlareImage;
    [SerializeField] private CanvasGroup _crateOpenEffect;
    [SerializeField] private RectTransform _crateRoot;
    [SerializeField] private RectTransform _crateTransform;
    [SerializeField] private RectTransform _bgRoot;
    [SerializeField] private Button _openCrateButton;
    [SerializeField] private Button _claimRewardButton;
    [SerializeField] private RectTransform _rewardItemContainer;
    [SerializeField] private RewardContainerUI _rewardContainerUI;
    [ReadOnly,SerializeField]
    private RewardProgressHolder _currentRewardProgressData;
    [SerializeField] private float _signalTimer = 0.3f;
    private UnityAction _onCompleteAction;
    private void Awake()
    {
        SignalBus.Subscribe<OnShowRewardProgressSignal>(OnShowClaimReward);
        _openCrateButton.onClick.AddListener(OpeningCrate);
        _claimRewardButton.onClick.AddListener(OnClaimReward);
    }

    void OnShowClaimReward(OnShowRewardProgressSignal signal)
    {
        _currentRewardProgressData = signal.RewardsData;
        _onCompleteAction = signal.OnRewardComplete;
        OnReset();
        ShowRewardsPanel();
    }

    public void OnReset()
    {
        _openCrateButton.gameObject.SetActive(true);
        _bgRoot.gameObject.SetActive(false);
        _rewardItemContainer.gameObject.SetActive(false);
        _claimRewardButton.gameObject.SetActive(false);
        _crateRoot.transform.localScale = Vector3.zero;
        _crateFlareImage.alpha = 0;
        _crateOpenEffect.alpha = 0;
    }

    void ShowRewardsPanel()
    {
        Sequence  seq = DOTween.Sequence();
        seq.AppendCallback(OnChangeCrateVisuals)
            .AppendInterval(0.5f)
            .AppendCallback(()=>{_bgRoot.gameObject.SetActive(true);})
            .Append(_crateRoot.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).From(Vector3.one * 0.5f))
            .AppendInterval(0.5f).AppendCallback(PlayPopLoopAnimation);
            DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.RewardPopUp);});



    }

    private Sequence _crateOpeningSequence;
    public void OpeningCrate()
    {
        _openCrateButton.gameObject.SetActive(false);
        if(_crateOpeningSequence != null )
            _crateOpeningSequence.Kill();

        if (popLoop != null)
        {
            popLoop.Rewind();
            popLoop.Kill();
        }

        float sequanceTime = 1f;
        _crateOpeningSequence = DOTween.Sequence();
        _crateOpeningSequence
            .Join(_crateRoot.transform.DOScale(Vector3.one * 1.3f, sequanceTime).SetEase(Ease.Linear).From(Vector3.one))
            .Join(_crateRoot.transform.DOShakePosition(sequanceTime, 5, 50,90,false,false)).SetEase(Ease.Linear)
            .Join(_crateFlareImage.DOFade(1, sequanceTime).From(0)).OnComplete(() =>
            {
                _crateFlareImage.DOFade(0, 0);
                _crateRoot.transform.DOScale(Vector3.one,0.3f);
                _mainCrateImage.transform.DOPunchScale(new Vector3(-0.5f, 0.5f, 0.5f), 0.3f, 0,1);
                PlayBlinkAndPopReward();
            });
    }

    private Sequence _crateOpenSequence;
    void OnCrateOpenEffect()
    {
        if(_crateOpeningSequence != null )
            _crateOpeningSequence.Kill();
        float sequanceTime = 0.5f;
        _crateOpenSequence = DOTween.Sequence();
        _crateOpenSequence
            .AppendCallback(PopUpRewardItems)
            .Join(_crateOpenEffect.transform.DOScale(10, sequanceTime).From(0))
            .Join(_crateOpenEffect.DOFade(0, sequanceTime).From(1));

    }

    void PopUpRewardItems()
    {
        AudioController.PlaySFX(AudioType.CannonTouch);
        foreach (var _reward in _currentRewardProgressData._rewards)
        {
            RewardContainerUI rewardContainer = Instantiate(_rewardContainerUI, _rewardItemContainer);
            rewardContainer.Init(_reward._rewardIcon,_reward.rewardChestAmount);
            rewardContainer.gameObject.SetActive(true);
            
        }
        _rewardItemContainer.gameObject.SetActive(true);
        _rewardItemContainer.DOAnchorPos(new Vector2(0,350f), 0.5f).SetEase(Ease.OutBack);
        _rewardItemContainer.DOScale(Vector3.one,0.5f).SetEase(Ease.OutBack).From(Vector3.zero);
        _claimRewardButton.gameObject.SetActive(true);
        _claimRewardButton.transform.DOScale(Vector3.one,0.5f).SetEase(Ease.OutBack).From(Vector3.one * 0.7f);
        
    }
    
    

    void PlayBlinkAndPopReward()
    {
        _mainCrateImage.sprite = _currentViewConfiguration.CrateIconOpen;
        AudioController.PlaySFX(AudioType.ChestOpen);
        OnCrateOpenEffect();
    }

    void OnClaimReward()
    {
        if (_currentRewardProgressData == null) return;

        var gameData = GlobalService.GameData;
        var bulkPopService = GlobalService.IBulkPopService;
        var spawnPos = _rewardItemContainer.transform.position;

        foreach (var reward in _currentRewardProgressData._rewards)
        {
            OnReset();

            int animatedCoinsAmount = 1;
            PopBulkService.BulkPopUpServiceType popType;

            switch (reward.rewardType)
            {
                case WeeklyRewardType.None:
                    continue;

                case WeeklyRewardType.PremiumCoin:
                    gameData.Data.PremiumCoins += reward.rewardChestAmount;
                    continue;

                case WeeklyRewardType.Hammer:
                    popType = PopBulkService.BulkPopUpServiceType.Hammer;
                    break;

                case WeeklyRewardType.InfiniteHealth:
                    popType = PopBulkService.BulkPopUpServiceType.Health;
                    animatedCoinsAmount = 10;
                    break;

                case WeeklyRewardType.Coin:
                    popType = PopBulkService.BulkPopUpServiceType.Coins;
                    animatedCoinsAmount = 10;
                    break;

                case WeeklyRewardType.MagicWand:
                    popType = PopBulkService.BulkPopUpServiceType.Wand;
                    break;

                case WeeklyRewardType.Magnet:
                    popType = PopBulkService.BulkPopUpServiceType.Magnets;
                    break;

                case WeeklyRewardType.PopTreasureBox:
                    popType = PopBulkService.BulkPopUpServiceType.SlotPopper;
                    break;

                default:
                    continue;
            }

            bulkPopService.PlayEffect(
                reward.rewardChestAmount,
                popType,
                spawnPos,
                animatedCoinsAmount,
                _onCompleteAction
            );
        }

        StartCoroutine(SendClaimSignalAfterDelay());
    }

    
    private IEnumerator SendClaimSignalAfterDelay()
    {
        yield return new WaitForSeconds(_signalTimer);
        SignalBus.Publish(new OnRewardClaimedSignal());
        SignalBus.Publish(new PuzzleRewardGiven(){});
    }


    private Sequence popLoop;
    void PlayPopLoopAnimation()
    {
        if (popLoop != null)
            popLoop.Kill();

        popLoop = DOTween.Sequence();

        popLoop.Append(
            _mainCrateImage.transform.DOPunchPosition(
                new Vector3(0, 20, 0), 
                1f, 
                1, 
                1
            ).SetEase(Ease.OutBounce)
        );

        popLoop.Join(
            _mainCrateImage.transform.DOPunchScale(
                new Vector3(-0.1f, 0, 0), 
                0.25f, 
                1, 
                0.245f
            ).SetEase(Ease.OutQuad)
        );

        popLoop.AppendInterval(0.5f);

        popLoop.SetLoops(-1, LoopType.Restart);
    }

    private CrassModelViewConfiguration _currentViewConfiguration;
    void OnChangeCrateVisuals()
    {
        _currentViewConfiguration = null;
        foreach (var config in _cratesModelConfigurationUI)
        {
            if (config.CrateType == _currentRewardProgressData._rewardCrateType){
                _currentViewConfiguration = config;
                break;
            }
        }

        if (_currentViewConfiguration == null)
        {
            Debug.LogError("No reward configuration found for "+ _currentRewardProgressData._rewardCrateType);
            return;
        }

        _mainCrateImage.sprite = _currentViewConfiguration.CrateIconClosed;
        _crateFlareImage.GetComponent<Image>().sprite = _currentViewConfiguration.CrateIconClosed;
        Color baseColor = _currentViewConfiguration.ColorValue;
        foreach (var flare in _backFlares)
        {
            Color c = flare.color;
            c.r = baseColor.r;
            c.g = baseColor.g;
            c.b = baseColor.b;
            flare.color = c;
        }

        
    }



    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnShowRewardProgressSignal>(OnShowClaimReward);
    }
    [System.Serializable]
    class CrassModelViewConfiguration
    {
        public RewardProgressModelView.RewardCrateType CrateType;
        public Color ColorValue;
        public Sprite CrateIconOpen;
        public Sprite CrateIconClosed;
    }
}


public class OnRewardClaimedSignal:ISignal
{
}

