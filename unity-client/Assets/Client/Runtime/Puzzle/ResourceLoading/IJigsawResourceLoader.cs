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

        Material OutlineGrid { get; }

        Material Shadow { get; }

        Material FullImageMaterial { get; }

        Transform Grid { get; }

        Transform FullImage { get; }

        UniTask LoadImageAsync(string key, CancellationToken cToken = default);

        void UnLoadImage();

        UniTask CreateGridAsync(string key, Transform parent, CancellationToken cToken = default);

        void DestroyGrid();

        UniTask CreateFullImageAsync(string key, Transform parent, CancellationToken cToken = default);

        void DestroyFullImage();

        UniTask LoadOutlineGridAsync(string key, CancellationToken cToken = default);

        void UnLoadOutlineGrid();
    }
}