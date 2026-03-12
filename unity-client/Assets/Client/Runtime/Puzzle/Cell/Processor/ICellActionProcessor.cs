namespace Client.Runtime
{
    public interface ICellActionProcessor
    {
        bool CanProcess { get; }

        void Process(ICellActionData data);

        string GetImageKey(ICellActionData data);
    }
}