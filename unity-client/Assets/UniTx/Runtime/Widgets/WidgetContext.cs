using System.Threading;
using UniTx.Runtime.IoC;

namespace UniTx.Runtime.Widgets
{
    /// <summary>
    /// Shared context for a widget and its states.
    /// </summary>
    /// <typeparam name="TData">The type of widget data.</typeparam>
    /// <typeparam name="TRefs">The type of widget references.</typeparam>
    public abstract class WidgetContext<TData, TRefs> : IInjectable
        where TData : IWidgetData
        where TRefs : WidgetRefs
    {
        /// <summary>
        /// Gets the runtime data associated with the widget.
        /// </summary>
        public TData Data { get; internal set; }

        /// <summary>
        /// Gets the hierarchy references associated with the widget.
        /// </summary>
        public TRefs Refs { get; internal set; }


        /// <summary>
        /// Gets the cancellation token for the widget's lifecycle.
        /// </summary>
        public CancellationToken CancellationTokenOnDestroy { get; internal set; }

        public virtual void Inject(IResolver resolver) { }
    }
}