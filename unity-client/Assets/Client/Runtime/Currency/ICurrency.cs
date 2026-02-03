using System;
using UniTx.Runtime.Entity;

namespace Client.Runtime
{
    public interface ICurrency : IEntity
    {
        event Action<ValueChangedData> OnValueChanged;

        double Amount { get; }

        void Add(double amount);

        void Remove(double amount);
    }

    public readonly struct ValueChangedData
    {
        public readonly double OldValue;
        public readonly double NewValue;

        public ValueChangedData(double oldValue, double newValue) => (OldValue, NewValue) = (oldValue, newValue);
    }
}