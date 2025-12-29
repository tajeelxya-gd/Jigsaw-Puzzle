using System.Collections.Generic;

namespace UniTx.Runtime.Content
{
    internal interface IDataLoader
    {
        IEnumerable<IData> Load(string json);
    }
}