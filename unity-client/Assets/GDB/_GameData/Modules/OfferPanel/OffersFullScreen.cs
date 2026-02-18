using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class OffersFullScreen : Offer
{
    [SerializeField] private Button _crossButton;
    [SerializeField] private AudioClip openSfxEffect;
    [SerializeField] private AudioClip closeSfxEffect;
  
    [ShowIf("@isTimedService == true")]
    [SerializeField] private int timeInSeconds = 7200;
    ITimeService offerTimer;
    private const string KEY = "offerTimer";
    
    [ReadOnly,SerializeField] bool _isTimerRunningDebug = false;
    protected override void OnEnable()
    {
        base.OnEnable();
        DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(openSfxEffect);});
        if(IsTimedOffer)
            StartTimer();
    }

    void StartTimer()
    {
        Debug.LogError("Timer Started for Offer "+ gameObject.name);
        offerTimer = new RealTimeService(KEY +  _offerData.OfferName,OnTimerEnded);
        if (!offerTimer.IsRunning())
            offerTimer.StartTimer(timeInSeconds);
        
    }
    private void Update()
    {
        _isTimerRunningDebug = IsTimerRunning();
    }

    void OnTimerEnded()
    {
        Debug.LogError("Timer Ended for Offer "+ gameObject.name);
    }
    public override bool IsTimerRunning()
    {
        if(offerTimer == null)
            offerTimer = new RealTimeService(KEY + _offerData.OfferName,OnTimerEnded);
        return offerTimer != null && offerTimer.IsRunning();
    }

    protected override void Awake()
    {
        base.Awake();
        if(_crossButton)
            _crossButton.onClick.AddListener(CloseOffer);
    }

    public override void CloseOffer()
    {
        base.CloseOffer();
        AudioController.PlaySFX(closeSfxEffect);
        if(canvasGroup)
            canvasGroup.DOFade(0, 0.25f).OnComplete(()=> { onOfferClosed?.Invoke(); });
        transform.DOScale(Vector3.one*0.75f, 0.5f ).OnComplete(()=>{gameObject.SetActive(false);});
        if(offerTimer != null && IsTimerRunning())
            SignalBus.Publish(new OnShowInAppHomeButton{OfferDataType = _offerData});
    }

    public override void BuyOffer()
    {
        base.BuyOffer();
        CloseOffer();
        if(offerTimer != null && IsTimerRunning())
            offerTimer.EndTimer();
    }

}
