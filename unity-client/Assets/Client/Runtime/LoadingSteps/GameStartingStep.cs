using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class GameStartingStep : LoadingStepBase
    {
        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            InputHandler.Init();
            return UniWidgets.PushAsync<MainMenuWidget>(PushLayer.Background, cToken);
        }
    }
}