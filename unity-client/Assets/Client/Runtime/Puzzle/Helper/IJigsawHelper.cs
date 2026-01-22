using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawHelper
    {
        Material BaseMaterial { get; }

        Material OutlineMaterial { get; }
        Material SemiOutlineMaterial { get; }

        void ToggleFullImage();

        UniTask LoadMaterialsAsync(string key, CancellationToken cToken = default);

        void UnLoadMaterials();
    }
}