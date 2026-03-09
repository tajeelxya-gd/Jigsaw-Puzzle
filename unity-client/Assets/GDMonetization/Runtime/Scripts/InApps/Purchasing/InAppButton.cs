using Monetization.Runtime.Logger;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Monetization.Runtime.InAppPurchasing
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class InAppButton : MonoBehaviour
    {
        [SerializeField] InAppProductSO productSO;
        [SerializeField] Button _button;
        [SerializeField] TextMeshProUGUI _text;
        private bool hasInitialized;

        #region Initialize

        private void OnEnable()
        {
            if (!hasInitialized)
            {
                hasInitialized = true;
                Initialize();
            }

            SetPriceLabel(productSO.IsPending);
            productSO.OnStateDeferred += SetPriceLabel;
        }

        
        private UnityAction _onOfferBought;
        public void RegisterBuyEvent(UnityAction onBuyAction)
        {
            _onOfferBought = onBuyAction;//for editor testing only
            productSO.RegisterOnPurchaseSuccessEvent(onBuyAction);
        }

        private void OnDisable()
        {
            productSO.OnStateDeferred -= SetPriceLabel;
        }

        void Initialize()
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(BuyProduct);
        }

        private void SetPriceLabel(bool isPending)
        {
            UpdateTextPriceTag(isPending ? "Pending" : productSO.Price);
        }

        protected virtual void UpdateTextPriceTag(string price)
        {
            if (_text != null)
            {
                _text.text = price;
            }
        }

        #endregion

        #region Process Purchasing

        private void BuyProduct()
        {
            if (GameStateGlobal.IsSandBox)
            {
                SignalBus.Publish(new OnInAppSuccessFullyBought{IsSuccess = true});
                _onOfferBought?.Invoke();
                return;
            }
            
            #if UNITY_EDITOR
                        Message.Log(Tag.InAppPurchasing, $"[EDITOR] Simulated purchase for {productSO.ProductId}");
                        _onOfferBought?.Invoke();
                        return;
            #endif

            if (productSO.IsPending)
            {
                Message.LogWarning(Tag.InAppPurchasing, $"BuyProduct: Product in pending state");
                return;
            }

            if (string.IsNullOrEmpty(productSO.ProductId))
            {
                Message.LogWarning(Tag.InAppPurchasing, $"Product ID is missing from {productSO.name}");
                return;
            }

            GDInAppPurchaseManager.Purchase(productSO.ProductId);
        }


        #endregion
    }
}