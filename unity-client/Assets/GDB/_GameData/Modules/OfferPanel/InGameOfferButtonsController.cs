using System;
using UnityEngine;

public class InGameOfferButtonsController : MonoBehaviour
{
   [SerializeField] private InGameOffersButton[] offerButtons;
   private void Start()
   {
       SignalBus.Subscribe<OnShowInAppHomeButton>(OnShowOfferButton);
       SignalBus.Subscribe<OnInAppBuySignal>(OnInAppBuySignal);
     DisplayAllTimedOffers();
   }

   void OnInAppBuySignal(OnInAppBuySignal signal)
   {
       foreach (var button in offerButtons)
       {
           button.gameObject.SetActive(button.OfferData.IsOfferAvailable()
           );
       }
   }

   void DisplayAllTimedOffers()
   {
       foreach (var button in offerButtons)
       {
           button.gameObject.SetActive(
               button.IsTimerRunning() 
               && button.OfferData.IsOfferAvailable()
           );
       }
   }

   void OnShowOfferButton(OnShowInAppHomeButton signal)
   {
       Debug.LogError("Requested To Show Offer Button:: "+signal.OfferDataType);
       foreach (var button in offerButtons)
       {
           button.gameObject.SetActive(
               button.OfferData ==  signal.OfferDataType &&
                button.OfferData.IsOfferAvailable()
           );
       }
   }


   private void OnDisable()
   {
       SignalBus.Unsubscribe<OnShowInAppHomeButton>(OnShowOfferButton);
       SignalBus.Unsubscribe<OnInAppBuySignal>(OnInAppBuySignal);

   }
}

public class OnShowInAppHomeButton : ISignal
{
    public InAppOfferData OfferDataType;
}
