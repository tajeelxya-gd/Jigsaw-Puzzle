public interface IGrid
{
    public void CreateColumns(int count, float distance);
    public void AddItemInColumn<T>(T item, int ind);
}