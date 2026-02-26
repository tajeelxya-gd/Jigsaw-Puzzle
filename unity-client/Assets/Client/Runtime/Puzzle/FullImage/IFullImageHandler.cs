using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public interface IFullImageHandler
    {
        bool IsActive();

        void ToggleFullImage();

        void ShowFullImage();

        void HideFullImage();

        UniTask PlayLevelCompletedAnimationAsync(CancellationToken cToken = default);
    }
}