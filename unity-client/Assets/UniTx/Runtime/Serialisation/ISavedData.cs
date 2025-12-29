namespace UniTx.Runtime.Serialisation
{
    /// <summary>
    /// Represents a persistable data object with an identifier and modified timestamp.
    /// </summary>
    public interface ISavedData
    {
        string Id { get; }

        long ModifiedTimestamp { get; set; }
    }
}