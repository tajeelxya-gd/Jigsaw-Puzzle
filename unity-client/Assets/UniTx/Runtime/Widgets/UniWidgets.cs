using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace UniTx.Runtime.Widgets
{
    public static class UniWidgets
    {
        private static IWidgetsManager _widgetsManager = null;

        internal static UniTask InitialiseAsync(IWidgetsManager widgetsManager, CancellationToken cToken = default)
        {
            _widgetsManager = widgetsManager ?? throw new ArgumentNullException(nameof(widgetsManager));
            return _widgetsManager.InitialiseAsync(cToken);
        }

        /// <summary>
        /// Triggered when a widget is pushed onto the stack.
        /// </summary>
        public static event Action<Type> OnPush
        {
            add => _widgetsManager.OnPush += value;
            remove => _widgetsManager.OnPush -= value;
        }

        /// <summary>
        /// Triggered when a widget is popped from the stack.
        /// </summary>
        public static event Action<Type> OnPop
        {
            add => _widgetsManager.OnPop += value;
            remove => _widgetsManager.OnPop -= value;
        }

        /// <summary>
        /// Asynchronously pushes a widget of the given type without data.
        /// </summary>
        public static UniTask PushAsync<TWidgetType>(CancellationToken cToken = default)
            where TWidgetType : IWidget
            => _widgetsManager.PushAsync<TWidgetType>(cToken);

        /// <summary>
        /// Asynchronously pushes a widget of the given type with data.
        /// </summary>
        public static UniTask PushAsync<TWidgetType>(IWidgetData widgetData, CancellationToken cToken = default)
            where TWidgetType : IWidget, IWidgetDataReceiver
            => _widgetsManager.PushAsync<TWidgetType>(widgetData, cToken);

        /// <summary>
        /// Asynchronously pops the widget from the stack.
        /// </summary>
        public static UniTask PopWidgetsStackAsync(CancellationToken cToken = default)
            => _widgetsManager.PopWidgetsStackAsync(cToken);

        /// <summary>
        /// Returns the widget currently at the top of the stack without removing it.
        /// Returns null if the widgets stack is empty.
        /// </summary>
        public static IWidget Peek() => _widgetsManager.Peek();
    }
}