using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniTx.Runtime
{
    /// <summary>
    /// Defines a synchronous reset capability.
    /// </summary>
    public interface IResettable
    {
        /// <summary>
        /// Resets the object.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Defines an asynchronous reset capability.
    /// </summary>
    public interface IResettableAsync
    {
        /// <summary>
        /// Resets the object asynchronously.
        /// </summary>
        UniTask ResetAsync(CancellationToken cToken = default);
    }
}