using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class MainMenuWidget : Widget<MainMenuWidgetContext, MainMenuWidgetData, MainMenuWidgetRefs>
    {
        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _stateMachine.RegisterState(new MainMenuWidgetDefaultState());
        }

        public override void Initialise()
        {
            base.Initialise();
            _stateMachine.SwitchState<MainMenuWidgetDefaultState>();
        }
    }
}
