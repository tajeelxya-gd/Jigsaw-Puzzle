using System;
using System.Collections.Generic;

namespace UniTx.Runtime.IoC
{
    internal sealed class UniContainer : IResolver, IBinder
    {
        private readonly Dictionary<Type, List<UniBinding>> _registry = new();

        /// <inheritdoc />
        public IBinding Bind<TConcrete>(TConcrete instance = null)
            where TConcrete : class, new()
        {
            return BindInternal(typeof(TConcrete), instance);
        }

        /// <inheritdoc />
        public IBinding Bind(Type type, object instance = null)
        {
            return BindInternal(type, instance);
        }

        /// <inheritdoc />
        public void Unbind<TConcrete>()
        {
            Unbind(typeof(TConcrete));
        }

        /// <inheritdoc />
        public void Unbind(Type type)
        {
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (_registry.TryGetValue(@interface, out var bindings))
                {
                    bindings.RemoveAll(b => b.ConcreteType == type);
                    if (bindings.Count == 0)
                    {
                        _registry.Remove(@interface);
                    }
                }
            }
        }

        /// <inheritdoc />
        public TContract Resolve<TContract>()
        {
            if (_registry.TryGetValue(typeof(TContract), out var bindings) && bindings.Count > 0)
            {
                return (TContract)bindings[0].GetInstance(this);
            }

            throw new InvalidOperationException($"No binding registered for type {typeof(TContract).Name}");
        }

        /// <inheritdoc />
        public IEnumerable<TContract> ResolveAll<TContract>()
        {
            if (_registry.TryGetValue(typeof(TContract), out var bindings))
            {
                foreach (var binding in bindings)
                {
                    yield return (TContract)binding.GetInstance(this);
                }
            }
        }

        private IBinding BindInternal(Type type, object instance)
        {
            var binding = new UniBinding(type, instance);

            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                RegisterBinding(@interface, binding);
            }

            return binding;
        }

        private void RegisterBinding(Type type, UniBinding binding)
        {
            if (!_registry.TryGetValue(type, out var bindings))
            {
                bindings = new List<UniBinding>();
                _registry[type] = bindings;
            }
            bindings.Add(binding);
        }
    }
}