using System;
using DG.Tweening;
using Monetization.Runtime.InAppPurchasing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Offer : MonoBehaviour
{
    [SerializeField] private protected InAppOfferData _offerData;
    public InAppOfferData OfferData => _offerData;
    public bool IsConsumeable => _offerData.IsConsumeable;
    protected CanvasGroup canvasGroup;
    private protected UnityAction<Offer> _onOfferBought;
    private protected UnityAction onOfferClosed;
    [SerializeField] private protected InAppButton inAppButton;
    [SerializeField] private bool isTimedService = false;
    public bool IsTimedOffer => isTimedService;
    public void Inject(UnityAction<Offer> onOfferBought, UnityAction onOfferClose)
    {
        _onOfferBought = onOfferBought;
        onOfferClosed = onOfferClose;
    }
    private void Start()
    {
        inAppButton?.RegisterBuyEvent(BuyOffer);
    }

    public virtual bool IsTimerRunning()
    {
        return false;
    }
    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

    }

    public string GetOfferName() => _offerData.OfferName;
    public virtual void CloseOffer() { }
    public virtual bool HasOffer() => _offerData != null && _offerData.IsOfferAvailable();
    public virtual void BuyOffer()
    {
        // Message.LogError(Tag.InAppPurchasing, $"[EDITOR] Offer purchased for {gameObject.name}");
        _offerData.BuyOffer();
        _onOfferBought?.Invoke(this);
    }

    protected virtual void OnEnable()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1;
        transform.localScale = Vector3.one;
    }


}
