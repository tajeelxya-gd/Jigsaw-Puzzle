using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniTx.Runtime
{
    /// <summary>
    /// Defines a synchronous initialisation capability.
    /// </summary>
    public interface IInitialisable
    {
        /// <summary>
        /// Initialises the object.
        /// </summary>
        void Initialise();
    }

    /// <summary>
    /// Defines an asynchronous initialisation capability.
    /// </summary>
    public interface IInitialisableAsync
    {
        /// <summary>
        /// Initialises the object asynchronously.
        /// </summary>
        UniTask InitialiseAsync(CancellationToken cToken = default);
    }
}