using System;
using Unity;
using Unity.Lifetime;
using UnityEngine;

namespace UniTx.Runtime.IoC
{
    internal sealed class UniBinder : IBinder
    {
        private readonly IUnityContainer _container;

        public UniBinder()
        {
            // Empty
        }

        public UniBinder(IUnityContainer container) => _container = container;

        public void BindAsSingleton<TConcrete>(TConcrete instance = null)
            where TConcrete : class, new()
            => BindInternal(typeof(TConcrete), instance ?? new TConcrete());

        public void BindAsSingleton(Type type, object instance = null)
        {
            if (type == null || !type.IsClass)
            {
                UniStatics.LogInfo("Type must be a non-null class type to bind.", this, Color.red);
                return;
            }

            if (instance == null)
            {
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    UniStatics.LogInfo($"Failed to create instance of {type.FullName}: {e.Message}", this, Color.red);
                    return;
                }
            }

            BindInternal(type, instance);
        }

        private void BindInternal(Type type, object instance)
        {
            var contracts = type.GetInterfaces();
            var len = contracts.Length;
            var name = type.FullName;

            for (var i = 0; i < len; i++)
            {
                var contract = contracts[i];

                if (_container.IsRegistered(contract, name))
                {
                    UniStatics.LogInfo(
                        $"Skipping duplicate binding: {contract.FullName} already registered with {name}.", this,
                        Color.red);
                    continue;
                }

                // Register unnamed (default)
                _container.RegisterInstance(contract, instance, new SingletonLifetimeManager());

                // Register named
                _container.RegisterInstance(contract, name, instance, new SingletonLifetimeManager());
            }
        }
    }
}