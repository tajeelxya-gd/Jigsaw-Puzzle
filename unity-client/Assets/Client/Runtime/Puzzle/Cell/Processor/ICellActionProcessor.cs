namespace Client.Runtime
{
    public interface ICellActionProcessor
    {
        void Process(ICellActionData data);

        string GetImageKey(ICellActionData data);
    }
}