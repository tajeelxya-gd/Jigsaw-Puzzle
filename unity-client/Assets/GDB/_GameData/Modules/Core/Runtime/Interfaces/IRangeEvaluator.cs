public interface IRangeEvaluator
{
    public bool isReached { get; set; }
    public bool CheckReached();
    public void OnReached(bool isFirst);
}
