namespace Client.Runtime
{
    public interface ICellActionProcessor
    {
        void Process(ICellActionData data);
    }
}