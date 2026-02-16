public interface IDataBase<T> where T : new()
{
    public void Save(T data, string _keyModifier = "");
    public T Load_Get(string _keyModifier = "");
#if UNITY_EDITOR
    public void Override(T value);
#endif
}
