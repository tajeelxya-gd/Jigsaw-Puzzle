using System;
using UnityEngine;
using UnityEngine.UI;

public class RestorePurchase : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnRestorePurchase);
    }

    void OnRestorePurchase()
    {
        //    GDInAppPurchaseManager.RestorePurchases(); 
    }
}
