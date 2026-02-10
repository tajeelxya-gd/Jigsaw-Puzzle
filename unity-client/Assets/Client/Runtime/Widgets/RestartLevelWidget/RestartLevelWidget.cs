using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class RestartLevelWidget : Widget<RestartLevelWidgetContext, RestartLevelWidgetData, RestartLevelWidgetRefs>
    {
        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _stateMachine.RegisterState(new RestartLevelWidgetDefaultState());
        }

        public override void Initialise()
        {
            base.Initialise();
            _stateMachine.SwitchState<RestartLevelWidgetDefaultState>();
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}