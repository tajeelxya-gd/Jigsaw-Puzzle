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
        IBinding Bind<TConcrete>(TConcrete instance = null)
            where TConcrete : class, new();

        /// <summary>
        /// Binds the specified <see cref="Type"/> and instance (or creates one) to all interfaces it implements.
        /// </summary>
        IBinding Bind(Type type, object instance = null);

        /// <summary>
        /// Unbinds all registered instances of <typeparamref name="TConcrete"/> from the container.
        /// </summary>
        void Unbind<TConcrete>();

        /// <summary>
        /// Unbinds all registered instances of the specified <see cref="Type"/> from the container.
        /// </summary>
        void Unbind(Type type);
    }
}
