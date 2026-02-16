using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Range(1, 5)] public int AvailableStartingSpaces = 5;
    [Range(1, 1000)] public int CoinRewardAmount = 10;
    [Range(0, 10)] public float ColumnDelayTime = 1;
    public bool HasGoal;
    public int GoalAmount;
    public LevelType levelType;
    public CannonData[] cannonData;
    public EnemyData[] enemyData;
    public BoxData[] boxData;
    public BoggieData[] boggiesData;


    [System.Serializable]
    public class CannonData
    {
        public CannonColumn[] cannonColumns;
        [System.Serializable]
        public class CannonColumn
        {
            public ShooterType shooterType = ShooterType.SimpleCannon;
            public ColorType colorType;
            public int ProjectileAmount;
            public bool isHidden;
            public bool isConnected = false;
            public int connectedColumn = -1;
            public int connectedRow = -1;
        }
    }
    [System.Serializable]
    public class EnemyData
    {
        public EnemyColumn[] enemyColumns;
        [System.Serializable]
        public class EnemyColumn
        {
            public ColorType colorType;
            public EnemyType enemyType;
            public int EnemyHealth = 1;
            public int PropID = -1;
            public int subPropID = -1;
        }
    }
    [System.Serializable]
    public class BoxData
    {
        public int PropID = -1;
        public int ColumnID, SecondColumnID;
        public EnemyInfo[] enemyData;

        [System.Serializable]
        public class EnemyInfo
        {
            public Enemy[] Enemies;

            [System.Serializable]
            public class Enemy
            {
                public ColorType colorType;
                public EnemyType enemyType;
            }
        }
    }
    [System.Serializable]
    public class BoggieData
    {
        public int BoggieID = -1;
        public ColorType colorType;
        public Boggie[] BoggieParts;
        public int MaxLastID = -1;

        [System.Serializable]
        public class Boggie
        {
            public int ID;
        }
    }
}