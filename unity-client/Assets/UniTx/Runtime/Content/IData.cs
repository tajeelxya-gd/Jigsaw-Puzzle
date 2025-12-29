using System;

namespace UniTx.Runtime.Content
{
    /// <summary>
    /// Base interface for data objects that can be retrieved from data services.
    /// </summary>
    public interface IData
    {
        public string Id { get; }
    }

    [Serializable]
    public class JsonArray<T>
    {
        public T[] Items;
    }
}