using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyInfo[] enemyInfos;

    public GameObject GetEnemy(EnemyType enemyType)
    {
        for (int i = 0; i < enemyInfos.Length; i++)
        {
            if (enemyInfos[i].enemyType == enemyType)
                return enemyInfos[i].enemyPrefab;
        }
        return null;
    }

    [System.Serializable]
    public class EnemyInfo
    {
        public GridItemType gridItemType;
        public EnemyType enemyType;
        [HorizontalGroup("Data", 200)]
        public GameObject enemyPrefab;
    }
}
public enum GridItemType
{
    StickMan,
    Boss,
    Collectable
}