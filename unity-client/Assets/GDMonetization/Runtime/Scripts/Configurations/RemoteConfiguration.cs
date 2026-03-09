using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monetization.Runtime.Configurations
{
    [CreateAssetMenu(fileName = "RemoteConfig", menuName = "GDMonetization/RemoteConfig")]

    [System.Serializable]
    public sealed class RemoteConfiguration : ScriptableObject
    {
        public bool ShowInternetPopup = true;
        public bool UseAdmobCollapsible = true;
        public bool DelayInterstitialOnRewardedAd = true;
        public bool DisableMaxOn2GBDevices = true;
        public int NextInterstitialDelay = 180;
        public int WallHealth = 2000;
        public int CoinMultiplier = 2;
        public int OfferPanelShowAfterLevels = 3;
        public bool ShowAdd = true;
        public int ShowAdAfter = 30;
    }
}