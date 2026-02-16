using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleManiaUIData", menuName = "Scriptable Objects/PuzzleManiaUIData")]
public class PuzzleManiaUIData : ScriptableObject
{
    public PuzzleManiaUIPreset[] _maniaUIPresets;
}

[Serializable]
public class PuzzleManiaUIPreset
{
    public UIData[] _maniaRewards;
}

[Serializable]
public class UIData
{
    public RewardProgressModelView _RewardProgress;
    public int _number;
    public int _requiredBlocks;
}