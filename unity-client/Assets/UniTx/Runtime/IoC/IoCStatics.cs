using UnityEngine;

namespace UniTx.Runtime.IoC
{
    public static class IoCStatics
    {
        public static IResolver Resolver { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Initialize()
        {
            var container = new UniContainer();
            container.Bind(container).AsSingleton();
            Resolver = container.Resolve<IResolver>();
        }
    }
}