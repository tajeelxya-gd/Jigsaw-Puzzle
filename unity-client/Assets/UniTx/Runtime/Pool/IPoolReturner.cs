namespace UniTx.Runtime.Pool
{
    public interface IPoolReturner
    {
        void Return(IPoolItem item);
    }
}