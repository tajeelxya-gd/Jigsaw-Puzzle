using System;

namespace UniTx.Runtime.Widgets
{
    /// <summary>
    /// A wrapper around the state machine specifically for widgets, 
    /// handling context injection for widget states.
    /// </summary>
    public class WidgetStateMachineWrapper<TContext, TData, TRefs> : IStateMachine
        where TContext : WidgetContext<TData, TRefs>
        where TData : IWidgetData
        where TRefs : WidgetRefs
    {
        private readonly IStateMachine _stateMachine = new StateMachine();
        private readonly TContext _context;

        public WidgetStateMachineWrapper(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IState CurrentState => _stateMachine.CurrentState;

        public void RegisterState(IState state)
        {
            if (state is WidgetState<TContext, TData, TRefs> widgetState)
            {
                widgetState.Prepare(_context);
            }
            _stateMachine.RegisterState(state);
        }

        public void UnregisterState<V>() where V : IState => _stateMachine.UnregisterState<V>();

        public void SwitchState<V>() where V : IState => _stateMachine.SwitchState<V>();
    }
}