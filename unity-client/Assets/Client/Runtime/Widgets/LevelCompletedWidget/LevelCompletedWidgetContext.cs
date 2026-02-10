using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public class LevelCompletedWidgetContext : WidgetContext<LevelCompletedWidgetData, LevelCompletedWidgetRefs>
    {
        public IPuzzleService PuzzleService { get; private set; }

        public override void Inject(IResolver resolver) => PuzzleService = resolver.Resolve<IPuzzleService>();
    }
}