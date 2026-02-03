using System;
using UniTx.Runtime.Extensions;

namespace Client.Runtime
{
    public class PropertyWrapper<T>
    {
        public event Action<T> OnChanged;

        public T Value { get; private set; }

        public PropertyWrapper(T value = default) => Value = value;

        public void Set(T value, bool notify = true)
        {
            Value = value;
            if (notify) OnChanged.Broadcast(value);
        }

        public static implicit operator T(PropertyWrapper<T> wrapper) => wrapper.Value;
        public static implicit operator PropertyWrapper<T>(T value) => new(value);
    }
}