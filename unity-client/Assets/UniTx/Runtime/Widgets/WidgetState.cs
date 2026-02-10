namespace UniTx.Runtime.Widgets
{
    /// <summary>
    /// Base class for states that belong to a specific widget type.
    /// Accesses shared widget context.
    /// </summary>
    public abstract class WidgetState<TContext, TData, TRefs> : IState
        where TContext : WidgetContext<TData, TRefs>
        where TData : IWidgetData
        where TRefs : WidgetRefs
    {
        /// <summary>
        /// The shared context of the widget.
        /// </summary>
        protected TContext Context { get; private set; }

        internal void Prepare(TContext context)
        {
            Context = context;
        }

        public abstract void OnEnter();

        public abstract void OnExit();
    }
}
