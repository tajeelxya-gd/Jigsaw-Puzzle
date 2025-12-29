namespace UniTx.Runtime.Events
{
    internal interface IListener
    {
        Priority Priority { get; }
    }
}