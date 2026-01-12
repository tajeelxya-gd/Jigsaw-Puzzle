using UniTx.Runtime.Content;

namespace Client.Runtime
{
    public interface ICellActionData : IData
    {
        int CellIdx { get; }
    }
}