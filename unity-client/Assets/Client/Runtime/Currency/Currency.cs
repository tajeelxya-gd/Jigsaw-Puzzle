using System;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public class Currency : EntityBase<CurrencyData, CurrencySavedData>, ICurrency
    {
        public event Action<ValueChangedData> OnValueChanged;

        public string ImageKey => Data.ImageKey;

        public double Amount => SavedData.Amount;

        public Currency(string id) : base(id)
        {
            // Empty
        }

        protected override void OnInject(IResolver resolver) { }

        protected override void OnInit() { }

        protected override void OnReset() { }

        public void Add(double amount)
        {
            if (amount <= 0) return;

            SetAmount(amount);
        }

        public void Remove(double amount)
        {
            if (amount <= 0) return;

            SetAmount(-amount);
        }

        private void SetAmount(double amount)
        {
            if (amount == 0) return;

            var oldValue = SavedData.Amount;
            var newValue = oldValue + amount;
            SavedData.SetAmount(newValue);
            Save();
            OnValueChanged.Broadcast(new ValueChangedData(oldValue, newValue));
        }
    }
}