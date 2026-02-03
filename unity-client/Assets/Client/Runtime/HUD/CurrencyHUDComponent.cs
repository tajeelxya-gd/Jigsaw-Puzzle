using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class CurrencyHUDComponent : MonoBehaviour, IInitialisable, IResettable, IInjectable
    {
        [SerializeField] private string _currencyId;
        [SerializeField] private TMP_Text _txtAmount;
        [SerializeField] private Image _currencyImg;

        private IEntityService _entityService;
        private ICurrency _currency;
        private Sprite _sprite;

        public void Inject(IResolver resolver) => _entityService = resolver.Resolve<IEntityService>();

        public void Initialise()
        {
            _currency = _entityService.Get<ICurrency>(_currencyId);
            UpdateText(_currency.Amount);
            RegisterEvents();
            LoadSpriteAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void Reset()
        {
            UnRegisterEvents();
            _currency = null;
            UpdateText(0);
        }

        private void RegisterEvents()
        {
            UniEvents.Subscribe<CurrencyCollectHudEffectEvent>(HandleCollectEffect);
            _currency.OnValueChanged += HandleValueChanged;
        }

        private void UnRegisterEvents()
        {
            UniEvents.Unsubscribe<CurrencyCollectHudEffectEvent>(HandleCollectEffect);
            _currency.OnValueChanged -= HandleValueChanged;
        }

        private void HandleValueChanged(ValueChangedData data) => UpdateText(data.NewValue);

        private void UpdateText(double amount) => _txtAmount.SetText($"{amount}");

        private async UniTask LoadSpriteAsync(CancellationToken cToken = default)
        {
            var spriteKey = _currency.ImageKey;
            _sprite = await UniResources.LoadAssetAsync<Sprite>(spriteKey, null, cToken);
            _currencyImg.sprite = _sprite;
        }

        private void UnloadSprite()
        {
            if (_sprite != null) UniResources.DisposeAsset(_sprite);
            _currencyImg.sprite = null;
        }

        private void HandleCollectEffect(CurrencyCollectHudEffectEvent @event)
        {
            if (!@event.ImageKey.Equals(_currency.ImageKey)) return;

            var duplicate = Instantiate(_sprite);
        }
    }
}