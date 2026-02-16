using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "ParticleData", menuName = "Scriptable Objects/ParticleData")]
public class ParticleData : ScriptableObject
{
    public ParticalInfo[] Data;
}
[System.Serializable]
public class ParticalInfo
{
    public ParticleType Type;
    public GameObject Particle;

    [HideInInspector] public ObjectPool<GameObject> Pool;
}