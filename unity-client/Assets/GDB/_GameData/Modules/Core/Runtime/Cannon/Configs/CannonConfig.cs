using UnityEngine;

[CreateAssetMenu(fileName = "CannonConfig", menuName = "Scriptable Objects/CannonConfig")]
public class CannonConfig : ScriptableObject
{
    [Range(0, 1)] public float DamagePerHit;
    [Range(0, 1)] public float RateOfFire;
    [Range(1, 20)] public int Range;
}
