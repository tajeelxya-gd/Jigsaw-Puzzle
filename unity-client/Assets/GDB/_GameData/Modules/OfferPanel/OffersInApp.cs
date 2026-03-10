using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class OffersInApp : Offer
{
  [SerializeField] private bool autoDisableState = false;
  [SerializeField] private bool hasAlternateState = false;
  [ShowIf(nameof(hasAlternateState))]
  [SerializeField] private GameObject alternateItemView;

  protected override void Awake()
  {
    base.Awake();
    SignalBus.Subscribe<OnInAppBuySignal>(OnInInAppBuy);
  }

  private void OnInInAppBuy(OnInAppBuySignal signal)
  {
    UpdateState();
  }

  protected override void OnEnable()
  {
    base.OnEnable();
    UpdateState();
  }

  void UpdateState()
  {
    if (this == null) return;
    if (autoDisableState) { gameObject.SetActive(HasOffer());}
    
    if(hasAlternateState)
      if (!HasOffer())
        if (alternateItemView)
        {
          alternateItemView.SetActive(true);
          gameObject.SetActive(false);
        }
          
  }

  public override void BuyOffer()
  {
    base.BuyOffer();
    UpdateState();
  }

  private void OnDisable()
  {
    SignalBus.Unsubscribe<OnInAppBuySignal>(OnInInAppBuy);
  }
}
