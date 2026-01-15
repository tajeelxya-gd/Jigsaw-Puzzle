using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public interface IPuzzleService
    {
        UniTask RestartPuzzleAsync(CancellationToken cToken = default);

        UniTask LoadPuzzleAsync(CancellationToken cToken = default);

        void UnLoadPuzzle();

        JigsawBoard GetCurrentBoard();

        JigSawLevelData GetCurrentLevelData();
    }
}
