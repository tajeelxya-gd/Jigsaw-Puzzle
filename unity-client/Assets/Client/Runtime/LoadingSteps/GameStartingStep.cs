using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class GameStartingStep : LoadingStepBase, IInjectable
    {
        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            InputHandler.Init();
            _puzzleService.LoadCurrentLevelData();
            return _puzzleService.LoadPuzzleAsync(cToken);
        }

    }
}