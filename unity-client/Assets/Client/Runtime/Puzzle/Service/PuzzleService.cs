using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService
    {
        public UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            return UniTask.CompletedTask;
        }
    }
}