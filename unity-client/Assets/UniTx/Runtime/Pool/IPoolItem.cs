namespace UniTx.Runtime.Pool
{
    public interface IPoolItem : IInitialisable, IResettable, ISceneEntity
    {
        void SetPoolReturner(IPoolReturner returner);

        void Return();
    }

    public interface IPoolItem<TData> : IPoolItem, IPoolItemDataReceiver
        where TData : IPoolItemData
    {
        TData Data { get; }
    }
}