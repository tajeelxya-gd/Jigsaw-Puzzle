using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class RestartLevelWidgetContext : WidgetContext<RestartLevelWidgetData, RestartLevelWidgetRefs>
    {
        public IPuzzleService PuzzleService { get; private set; }

        public override void Inject(IResolver resolver) => PuzzleService = resolver.Resolve<IPuzzleService>();
    }
}
