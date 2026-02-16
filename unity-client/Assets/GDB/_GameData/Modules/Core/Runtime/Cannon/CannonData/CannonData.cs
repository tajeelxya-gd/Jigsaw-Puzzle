using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CannonData", menuName = "Scriptable Objects/CannonData")]
public class CannonData : ScriptableObject
{
    public CannonInfo[] cannons;

    public CannonInfo GetCannonInfo(ShooterType cannonType)
    {
        return cannons.FirstOrDefault(c => c.cannonType == cannonType);
    }

    [System.Serializable]
    public class CannonInfo
    {
        public ShooterType cannonType;
        public Projectile projectile;
        public Cannon[] Cannons;

        public Cannon GetCannonPrefab(int UpdateID)
        {
            if (Cannons.Length == 0) return null;
            int id = Mathf.Clamp(UpdateID, 0, Cannons.Length);
            return Cannons[id];
        }
    }
}
public enum ShooterType
{
    SimpleCannon,
    MissileCannon,
    LockedCannon
}