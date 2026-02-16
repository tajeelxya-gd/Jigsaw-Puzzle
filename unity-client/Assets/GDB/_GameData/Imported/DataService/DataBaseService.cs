using System.IO;
using UnityEngine;
public class DataBaseService<T> : IDataBase<T> where T : new()
{
    private T _data;
    private string _dataPath
    {
        get
        {
            return $"{Application.persistentDataPath}/{typeof(T).Name}{_key}.json";
        }
    }

    private string _key = "";
    public T Load_Get(string _keyModifier = "")
    {
        _key = _keyModifier;
        if (File.Exists(_dataPath))
        {
            string json = File.ReadAllText(_dataPath);
            _data = JsonUtility.FromJson<T>(json);
        }
        else
        {
            _data = new T();
            Save(_data, _key);
        }
        return _data;
    }
    public void Save(T data, string _keyModifier = "")
    {
        _data = data;
        string json = JsonUtility.ToJson(_data, true);
        if (_data == null)
        {
            Debug.LogError("No data to save!");
            return;
        }
        _key = _keyModifier;
        File.WriteAllText(_dataPath, json);
    }

#if UNITY_EDITOR
    public void Override(T value)
    {
        _data = value;
        Save(_data);
    }
#endif
}
