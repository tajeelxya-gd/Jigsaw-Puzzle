using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace UniTx.Runtime.Widgets
{
    /// <summary>
    /// Manages in-game UI widgets, including pushing, popping based on the provided database.
    /// </summary>
    public interface IWidgetsManager : IInitialisableAsync
    {
        /// <summary>
        /// Triggered when a widget is pushed onto the stack.
        /// </summary>
        event Action<Type> OnPush;

        /// <summary>
        /// Triggered when a widget is popped from the stack.
        /// </summary>
        event Action<Type> OnPop;

        /// <summary>
        /// Asynchronously pushes a widget of the given type without data.
        /// </summary>
        UniTask PushAsync<TWidgetType>(CancellationToken cToken = default)
            where TWidgetType : IWidget;

        /// <summary>
        /// Asynchronously pushes a widget of the given type with data.
        /// </summary>
        UniTask PushAsync<TWidgetType>(IWidgetData widgetData, CancellationToken cToken = default)
            where TWidgetType : IWidget, IWidgetDataReceiver;

        /// <summary>
        /// Asynchronously pops the widget from the stack.
        /// </summary>
        UniTask PopWidgetsStackAsync(CancellationToken cToken = default);

        /// <summary>
        /// Returns the widget currently at the top of the stack without removing it.
        /// Returns null if the widgets stack is empty.
        /// </summary>
        IWidget Peek();
    }
}