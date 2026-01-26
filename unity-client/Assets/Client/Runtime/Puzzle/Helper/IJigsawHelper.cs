using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawHelper
    {
        Material PieceMaterial { get; }

        Material PieceBaseMaterial { get; }

        void ToggleFullImage();

        UniTask LoadMaterialsAsync(string key, CancellationToken cToken = default);

        void UnLoadMaterials();
    }
}