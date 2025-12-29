namespace UniTx.Runtime.Content
{
    internal interface IProcessStrategy
    {
        void Process(IData data);
    }
}