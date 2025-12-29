using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace UniTx.Runtime.Content
{
    public interface IContentLoader
    {
        /// <summary>
        /// Asynchronously loads all content associated with the given tags.
        /// </summary>
        /// <param name="tags">Tags used to determine which content to load.</param>
        /// <param name="cToken">Token to cancel the load operation.</param>
        UniTask LoadContentAsync(IEnumerable<string> tags, CancellationToken cToken = default);

        /// <summary>
        /// Asynchronously unloads all content associated with the given tags.
        /// </summary>
        /// <param name="tags">Tags used to determine which content to load.</param>
        /// <param name="cToken">Token to cancel the load operation.</param>
        UniTask UnloadContentAsync(IEnumerable<string> tags, CancellationToken cToken = default);
    }
}