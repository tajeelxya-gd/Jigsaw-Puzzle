using System;

namespace UniTx.Runtime.IoC
{
    /// <summary>
    /// Defines a binder that can register concrete instances against their interfaces in the underlying dependency injection container.
    /// </summary>
    public interface IBinder
    {
        /// <summary>
        /// Binds an instance of <typeparamref name="TConcrete"/> (or creates one) to all interfaces it implements.
        /// </summary>
        void BindAsSingleton<TConcrete>(TConcrete instance = null)
            where TConcrete : class, new();

        /// <summary>
        /// Binds the specified <see cref="Type"/> and instance (or creates one) to all interfaces it implements.
        /// </summary>
        void BindAsSingleton(Type type, object instance = null);
    }
}