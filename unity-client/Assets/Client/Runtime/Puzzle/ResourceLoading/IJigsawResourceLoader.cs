using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawResourceLoader
    {
        Material Base { get; }

        Material OutlineTray { get; }

        Material OutlineBoard { get; }

        Transform Grid { get; }

        UniTask LoadImageAsync(string key, CancellationToken cToken = default);

        void UnLoadImage();

        UniTask CreateGridAsync(string key, Transform parent, CancellationToken cToken = default);

        void DestroyGrid();
    }
}