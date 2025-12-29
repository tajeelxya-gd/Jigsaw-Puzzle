namespace UniTx.Runtime.Events
{
    /// <summary>
    /// Order in which delegates will be invoked.
    /// </summary>
    public enum Priority
    {
        Medium = 0,
        Lowest,
        Low,
        High,
        Highest
    }
}