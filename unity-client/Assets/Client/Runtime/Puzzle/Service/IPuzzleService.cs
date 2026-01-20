using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleService
    {
        Transform PuzzleRoot { get; }
        Transform FrameMesh { get; }

        UniTask RestartPuzzleAsync(CancellationToken cToken = default);

        UniTask LoadPuzzleAsync(CancellationToken cToken = default);

        void UnLoadPuzzle();

        JigsawBoard GetCurrentBoard();

        JigSawLevelData GetCurrentLevelData();
        JigSawLevelData GetNextLevelData();

        void LoadCurrentLevelData();
    }
}
