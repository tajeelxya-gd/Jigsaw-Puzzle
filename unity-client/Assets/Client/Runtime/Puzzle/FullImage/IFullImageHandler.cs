using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public interface IFullImageHandler
    {
        void ToggleFullImage();

        UniTask PlayLevelCompletedAnimationAsync(CancellationToken cToken = default);
    }
}