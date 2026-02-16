using System;
using System.Collections.Generic;
using System.Linq;
using Monetization.Runtime.RemoteConfig;
using Sirenix.OdinInspector;
using UnityEngine;

public class OfferPanelsController : MonoBehaviour
{
    [SerializeField] private OffersFullScreen[] _allOffers;

    [Header("Offer Display Rules")]
    public int showOfferEveryNEntries = 0;

    [ReadOnly, SerializeField]
    private List<Offer> availableOffers = new List<Offer>();

    [SerializeField] private bool isTest = false;

    // Keys for persistence
    const string ENTRY_COUNT_KEY = "OfferSceneEntryCount";
    const string NEXT_SEQUENTIAL_INDEX_KEY = "OfferNextIndex";
    const string LAST_TIMED_OFFER_ID_KEY = "LastTimedOfferID";

    private void Start()
    {
        SetUp();

        if (ShouldShowOfferThisEntry())
        {
            PopCommandExecutionResponder.AddCommand(new OfferPanelsShowCommand(
                PopCommandExecutionResponder.PopupPriority.Medium,
                executionCommand => ShowNextAvailableOffer()));
        }
    }

    [Button]
    void testCallOffers()
    {
        ShowNextAvailableOffer();
    }
    void OnCloseOffer() => CloseOnBoardingCommandIfYes();

    void SetUp()
    {
        SignalBus.Subscribe<ShowFullScreenOfferOnDemand>(OnShowFullScreenOfferOnDemand);

        if(!isTest && RemoteConfigManager.Configuration != null)
            showOfferEveryNEntries = RemoteConfigManager.Configuration.OfferPanelShowAfterLevels + 1;

        ResetAllAvailableOffers();

        foreach (var offer in availableOffers)
        {
            offer.gameObject.SetActive(false);
            offer.Inject(OnOfferBought, OnCloseOffer);
        }
    }

    void CloseOnBoardingCommandIfYes()
    {
       // if (PopCommandExecutionResponder.HasCommand<OfferPanelsShowCommand>())
            PopCommandExecutionResponder.RemoveCommand<OfferPanelsShowCommand>();
    }

    void ResetAllAvailableOffers()
    {
        availableOffers.Clear();
        foreach (var offer in _allOffers)
        {
            if (!offer.IsConsumeable && !offer.HasOffer()) continue;
            availableOffers.Add(offer);
        }
    }

    #region 🔥 SEQUENTIAL DISPLAY LOGIC

    void ShowNextAvailableOffer()
    {
        if (availableOffers.Count == 0) return;

        int currentIndex = PlayerPrefs.GetInt(NEXT_SEQUENTIAL_INDEX_KEY, 0);
        Offer runningTimed = GetRunningTimedOffer();
        int lastTimedID = PlayerPrefs.GetInt(LAST_TIMED_OFFER_ID_KEY, -1);

        int attempts = 0;
        int totalOffers = availableOffers.Count;

        while (attempts < totalOffers)
        {
            // Modular mapping to ensure we stay within bounds
            int targetIndex = (currentIndex + attempts) % totalOffers;
            Offer candidate = availableOffers[targetIndex];

            if (!candidate.IsTimedOffer)
            {
                DisplayOffer(candidate, targetIndex);
                return;
            }

            // 2. Timed Offer
            if (candidate.IsTimedOffer)
            {
                // A: A timer is already running
                if (runningTimed != null)
                {
                    if (candidate == runningTimed)
                    {
                        DisplayOffer(candidate, targetIndex);
                        return;
                    }
                    attempts++;
                    continue; 
                }
                
                // B: No timer is running. Check if we should start THIS one.
                int candidateID = candidate.name.GetHashCode(); 
                
                // If it's the one that JUST expired, try to skip to the next available one
                if (candidateID == lastTimedID && availableOffers.Count(o => o.IsTimedOffer) > 1)
                {
                    attempts++;
                    continue; 
                }

                PlayerPrefs.SetInt(LAST_TIMED_OFFER_ID_KEY, candidateID);
                DisplayOffer(candidate, targetIndex);
                return;
            }
            
            attempts++;
        }

        // Fallback: If everything was skipped (shouldn't happen with untimed offers present)
        DisplayOffer(availableOffers[currentIndex % totalOffers], currentIndex % totalOffers);
    }

    void DisplayOffer(Offer offer, int lastUsedIndex)
    {
        offer.gameObject.SetActive(true);
        // Set the index for the NEXT session to be the one after this one
        PlayerPrefs.SetInt(NEXT_SEQUENTIAL_INDEX_KEY, (lastUsedIndex + 1) % availableOffers.Count);
    }

    Offer GetRunningTimedOffer() => availableOffers.FirstOrDefault(o => o.IsTimedOffer && o.IsTimerRunning());

    #endregion

    bool ShouldShowOfferThisEntry()
    {
        if (availableOffers.Count == 0) return false;
        if (showOfferEveryNEntries <= 0) return true;

        int entryCount = PlayerPrefs.GetInt(ENTRY_COUNT_KEY, 0) + 1;
        PlayerPrefs.SetInt(ENTRY_COUNT_KEY, entryCount);

        return entryCount % showOfferEveryNEntries == 0;
    }

    public void OnOfferBought(Offer offer)
    {
        if (!offer.IsConsumeable) 
            availableOffers.Remove(offer);
            
        offer.gameObject.SetActive(false);
        // Reset index to 0 because the list order has changed
        PlayerPrefs.SetInt(NEXT_SEQUENTIAL_INDEX_KEY, 0); 
    }

    void OnShowFullScreenOfferOnDemand(ShowFullScreenOfferOnDemand signal)
    {
        foreach (var offer in availableOffers)
            offer.gameObject.SetActive(offer.HasOffer() && offer.OfferData == signal.OfferData);
    }

    private void OnDisable() => SignalBus.Unsubscribe<ShowFullScreenOfferOnDemand>(OnShowFullScreenOfferOnDemand);

    [Button]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(ENTRY_COUNT_KEY);
        PlayerPrefs.DeleteKey(NEXT_SEQUENTIAL_INDEX_KEY);
        PlayerPrefs.DeleteKey(LAST_TIMED_OFFER_ID_KEY);
        Debug.Log("Sequence Reset to 0");
    }
}
public class OnInAppBuySignal : ISignal
{
    public string Id = "";
}

public class ShowFullScreenOfferOnDemand : ISignal
{
    public InAppOfferData OfferData;
}