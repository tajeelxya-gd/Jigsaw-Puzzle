public interface IColumn<T>
{
    void InitializeColumns(int columnCount);
    void AddItem(int columnIndex, T cannon);
    void OnRemove(int columnIndex);
}