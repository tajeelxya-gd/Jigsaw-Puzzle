using Cysharp.Threading.Tasks;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace UniTx.Runtime.Widgets
{
    /// <summary>
    /// Base implementation for a widget with internal state management and shared context.
    /// </summary>
    /// <typeparam name="TContext">Context type associated with the widget.</typeparam>
    /// <typeparam name="TData">Data type associated with the widget.</typeparam>
    /// <typeparam name="TRefs">References type containing UI element links.</typeparam>
    public abstract class Widget<TContext, TData, TRefs> : MonoBehaviour, IWidget<TData>, IInjectable
        where TContext : WidgetContext<TData, TRefs>, new()
        where TData : IWidgetData
        where TRefs : WidgetRefs
    {
        [Header("Widget Refs")]
        [SerializeField] private TRefs _refs;

        protected TContext _context;
        protected WidgetStateMachineWrapper<TContext, TData, TRefs> _stateMachine;

        /// <summary>
        /// Gets the current data of the widget.
        /// </summary>
        public TData Data => _context.Data;

        /// <summary>
        /// Gets the underlying GameObject.
        /// </summary>
        public GameObject GameObject => gameObject;

        /// <summary>
        /// Gets the Transform components.
        /// </summary>
        public Transform Transform => transform;

        protected virtual void Awake()
        {
            _context = new TContext();
            _stateMachine = new WidgetStateMachineWrapper<TContext, TData, TRefs>(_context);
            _context.Refs = _refs;
        }

        public virtual void Inject(IResolver resolver)
        {
            _context.Inject(resolver);
        }

        /// <summary>
        /// Initialises the widget. Override this to register states and switch to the initial state.
        /// </summary>
        public virtual void Initialise()
        {
            _context.CancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
        }

        /// <summary>
        /// Resets the widget when it is popped from the stack.
        /// Ensures the current state is exited.
        /// </summary>
        public virtual void Reset()
        {
            _stateMachine.CurrentState?.OnExit();
            _context.Data = default;
        }

        /// <summary>
        /// Sets the runtime data for the widget.
        /// </summary>
        public void SetData(IWidgetData data)
        {
            if (data is TData tData)
            {
                _context.Data = tData;
            }
        }
    }
}
