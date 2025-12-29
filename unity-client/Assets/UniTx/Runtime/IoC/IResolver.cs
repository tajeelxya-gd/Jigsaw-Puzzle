using System.Collections.Generic;

namespace UniTx.Runtime.IoC
{
    /// <summary>
    /// Provides methods to resolve single or multiple concrete instances by contract type.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolves a single instance of the specified contract type.
        /// </summary>
        TContract Resolve<TContract>();

        /// <summary>
        /// Resolves all registered instances of the specified contract type.
        /// </summary>
        IEnumerable<TContract> ResolveAll<TContract>();
    }
}