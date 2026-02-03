using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class CurrencyHUDComponent : MonoBehaviour, IInitialisable, IResettable, IInjectable
    {
        [SerializeField] private string _currencyId;
        [SerializeField] private TMP_Text _txtAmount;

        private IEntityService _entityService;
        private ICurrency _currency;

        public void Inject(IResolver resolver) => _entityService = resolver.Resolve<IEntityService>();

        public void Initialise()
        {
            _currency = _entityService.Get<ICurrency>(_currencyId);
            UpdateText(_currency.Amount);
            _currency.OnValueChanged += HandleValueChanged;
        }

        public void Reset()
        {
            _currency.OnValueChanged -= HandleValueChanged;
            _currency = null;
            UpdateText(0);
        }

        private void HandleValueChanged(ValueChangedData data) => UpdateText(data.NewValue);

        private void UpdateText(double amount) => _txtAmount.SetText($"{amount}");
    }
}