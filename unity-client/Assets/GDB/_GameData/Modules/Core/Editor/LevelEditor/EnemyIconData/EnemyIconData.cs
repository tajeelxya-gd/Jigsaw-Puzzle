using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyIconData", menuName = "Game/EnemyIconData")]
public class EnemyIconData : ScriptableObject
{
    public List<Data> icons = new List<Data>();

    private Dictionary<EnemyType, Texture2D> _lookup;

    [System.Serializable]
    public class Data
    {
        public EnemyType enemyType;
        public Texture2D icon;
    }

    private void OnEnable()
    {
        _lookup = new Dictionary<EnemyType, Texture2D>();

        foreach (var data in icons)
        {
            if (!_lookup.ContainsKey(data.enemyType))
                _lookup.Add(data.enemyType, data.icon);
        }
    }

    public Texture2D GetIcon(EnemyType type)
    {
        if (_lookup == null)
            OnEnable();

        return _lookup.TryGetValue(type, out Texture2D icon) ? icon : null;
    }
}