using System;
using UnityEngine;
using UnityEngine.UI;

public class InGameOffersButton : MonoBehaviour
{

    [SerializeField] private InAppOfferData offerData;
    public InAppOfferData OfferData =>  offerData;
    [SerializeField] private Button offerButton;
    private const string KEY = "offerTimer";

    private void Awake()
    {
        offerButton.onClick.AddListener(RequestOfferAgain);
    }

    void RequestOfferAgain()
    {
        SignalBus.Publish(new ShowFullScreenOfferOnDemand{OfferData =offerData });
    }
    ITimeService offerTimer;
    public bool IsTimerRunning()
    {
        if(offerTimer == null)
            offerTimer = new RealTimeService(KEY + offerData.OfferName,null);
        return offerTimer != null && offerTimer.IsRunning();
    }

}
