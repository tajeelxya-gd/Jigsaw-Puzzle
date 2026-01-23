using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawHelper
    {
        Material BaseMaterial { get; }

        Material PieceTrayOutline { get; }
        Material PieceBoardOutline { get; }

        void ToggleFullImage();

        UniTask LoadMaterialsAsync(string key, CancellationToken cToken = default);

        void UnLoadMaterials();
    }
}