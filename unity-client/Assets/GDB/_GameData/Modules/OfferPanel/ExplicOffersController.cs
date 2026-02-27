using System;
using System.Linq;
using UnityEngine;

public class ExplicOffersController : MonoBehaviour
{
    [SerializeField] Offer[] explicOffer;
    void SetUp()
    {
        foreach (Offer offer in explicOffer)
            offer.Inject(OnOfferBought, OnOfferClosed);
    }

    void OnOfferBought(Offer offer)
    {

    }
    private const string RemoveAdsOfferName = "RemoveAds";

    private int minAdIterationCount = 3;
    bool HasValidAddSession()
    {
        GameData gameData = GlobalService.GameData;
        if (gameData.Data.CurrentInterstitialCount == 0) return false;
        return gameData.Data.CurrentInterstitialCount % minAdIterationCount == 0;
    }

    void ShowRemoveAdOfferIfAvailable()
    {
        if (!GameStateGlobal.AdShownSuccessfully) return;
        GameStateGlobal.AdShownSuccessfully = false;

        var offer = explicOffer
            .FirstOrDefault(o => o.GetOfferName() == RemoveAdsOfferName);

        if (offer == null || !offer.HasOffer()) return;

        if (!HasValidAddSession()) return;
        PopCommandExecutionResponder.AddCommand(
            new OnShowExplicitOfferCommand(
                PopCommandExecutionResponder.PopupPriority.Critical,
                execution =>
                {
                    SignalBus.Publish(new ShowExplicitOffer
                    {
                        OfferName = RemoveAdsOfferName
                    });
                })
        );


    }

    void OnOfferClosed()
    {
        PopCommandExecutionResponder.RemoveCommand<OnShowExplicitOfferCommand>();
    }
    private void Start()
    {
        SetUp();
        SignalBus.Subscribe<ShowExplicitOffer>(OnOfferPanelRequested);
        ShowRemoveAdOfferIfAvailable();
        //DontDestroyOnLoad(gameObject);
    }

    void OnOfferPanelRequested(ShowExplicitOffer signal)
    {
        foreach (var offer in explicOffer)
        {
            if (String.CompareOrdinal(offer.GetOfferName(), signal.OfferName) == 0)
            {
                offer.gameObject.SetActive(true);
                return;
            }
        }
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<ShowExplicitOffer>(OnOfferPanelRequested);
    }
}

public class ShowExplicitOffer : ISignal
{
    public string OfferName;
}