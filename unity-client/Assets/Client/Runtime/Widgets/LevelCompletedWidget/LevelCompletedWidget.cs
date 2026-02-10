using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public sealed class LevelCompletedWidget : Widget<LevelCompletedWidgetContext, LevelCompletedWidgetData, LevelCompletedWidgetRefs>
    {
        public override void Inject(IResolver resolver)
        {
            base.Inject(resolver);
            _stateMachine.RegisterState(new LevelCompletedWidgetDefaultState());
        }

        public override void Initialise()
        {
            base.Initialise();
            _stateMachine.SwitchState<LevelCompletedWidgetDefaultState>();
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}
