namespace UniTx.Runtime.Widgets
{
    /// <summary>
    /// Base for UI Widget entity.
    /// </summary>
    public interface IWidget : IInitialisable, IResettable, ISceneEntity
    {
        // Empty
    }

    public interface IWidget<TData> : IWidget, IWidgetDataReceiver
        where TData : IWidgetData
    {
        TData Data { get; }
    }
}