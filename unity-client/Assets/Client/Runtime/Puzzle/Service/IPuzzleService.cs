using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleService
    {
        Transform PuzzleRoot { get; }

        Transform FrameMesh { get; }

        UniTask RestartPuzzleAsync(bool reloadState, CancellationToken cToken = default);

        UniTask LoadPuzzleAsync(bool reloadState, CancellationToken cToken = default);

        void UnLoadPuzzle();

        JigsawBoard GetCurrentBoard();

        JigsawLevelData GetCurrentLevelData();

        JigsawLevelData GetNextLevelData();

        float RemainingTime { get; }
        event System.Action<float> OnTimerTick;
    }
}
