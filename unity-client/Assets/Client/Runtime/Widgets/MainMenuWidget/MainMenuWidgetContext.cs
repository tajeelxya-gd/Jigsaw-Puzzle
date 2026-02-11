using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class MainMenuWidgetContext : WidgetContext<MainMenuWidgetData, MainMenuWidgetRefs>
    {
        public IPuzzleService PuzzleService { get; private set; }

        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            PuzzleService = resolver.Resolve<IPuzzleService>();
        }
    }
}
