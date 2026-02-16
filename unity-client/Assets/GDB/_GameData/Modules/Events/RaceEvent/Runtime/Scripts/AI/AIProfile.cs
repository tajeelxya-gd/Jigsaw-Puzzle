using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AIProfile", menuName = "Scriptable Objects/AIProfile")]
public class AIProfile : ScriptableObject
{
    public AiData[] _aiData;
}
[Serializable]
public class AiData
{
    public string _name;
    public Sprite _profilePicture;
}

