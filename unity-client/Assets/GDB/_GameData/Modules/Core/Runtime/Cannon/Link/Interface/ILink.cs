public interface ILink<T>
{
    public bool IsLinked { get; set; }
    public bool AllFree { get; }
    public T LinkedObject { get; }
    public void Link(T item, bool linkAgain = false);
}