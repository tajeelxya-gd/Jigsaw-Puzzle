using System.Collections.Generic;
using Unity;

namespace UniTx.Runtime.IoC
{
    internal sealed class UniResolver : IResolver
    {
        private readonly IUnityContainer _container;

        public UniResolver()
        {
            // Empty
        }

        public UniResolver(IUnityContainer container) => _container = container;

        public TContract Resolve<TContract>() => _container.Resolve<TContract>();

        public IEnumerable<TContract> ResolveAll<TContract>() => _container.ResolveAll<TContract>();
    }
}