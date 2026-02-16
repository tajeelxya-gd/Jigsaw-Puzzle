using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    public AnimationCurve KillEffectCurve;
    [Range(0, 3)] public float Health;
    [Range(0, 10)] public float Speed;
    [Range(0, 10)] public int Damage;
    public AnimationCurve SpeedCurve;
}