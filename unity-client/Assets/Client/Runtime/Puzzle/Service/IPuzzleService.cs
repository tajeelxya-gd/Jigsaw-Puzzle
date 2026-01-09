using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleService
    {
        UniTask LoadPuzzleAsync(CancellationToken cToken = default);

        void UnLoadPuzzle();

        JigsawBoard GetCurrentBoard();
    }
}
