using System;

namespace UniTx.Runtime.IoC
{
    /// <summary>
    /// Internal implementation of the <see cref="IBinding"/> interface.
    /// </summary>
    internal sealed class UniBinding : IBinding
    {
        /// <summary>
        /// Gets the concrete type that this binding resolves to.
        /// </summary>
        public Type ConcreteType { get; }
        private object _instance;
        private bool _isSingleton = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniBinding"/> class.
        /// </summary>
        /// <param name="concreteType">The concrete type to bind.</param>
        /// <param name="instance">An optional pre-existing instance to use for singletons.</param>
        public UniBinding(Type concreteType, object instance)
        {
            ConcreteType = concreteType;
            _instance = instance;
        }

        /// <inheritdoc />
        public IBinding AsSingleton()
        {
            _isSingleton = true;
            return this;
        }

        /// <inheritdoc />
        public IBinding AsTransient()
        {
            _isSingleton = false;
            return this;
        }

        /// <inheritdoc />
        public void Conclude()
        {
            if (_isSingleton && _instance == null)
            {
                _instance = Activator.CreateInstance(ConcreteType);
            }
        }

        /// <summary>
        /// Resolves the instance for this binding using the provided resolver.
        /// </summary>
        /// <param name="resolver">The resolver to use for dependency resolution (if applicable).</param>
        /// <returns>The resolved instance.</returns>
        public object GetInstance(IResolver resolver)
        {
            if (_isSingleton)
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance(ConcreteType);
                }
                return _instance;
            }

            return Activator.CreateInstance(ConcreteType);
        }
    }
}