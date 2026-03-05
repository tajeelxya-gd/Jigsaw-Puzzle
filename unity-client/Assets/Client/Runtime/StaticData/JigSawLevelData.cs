using System;
using UniTx.Runtime.Content;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class JigsawLevelData : IData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _name;
        [SerializeField] private string _gridId;
        [SerializeField] private string _imageKey;
        [SerializeField] private int _difficulty;
        [SerializeField] private string[] _cellActionIds;
        [SerializeField] private int _maxDuration;
        [SerializeField] private string _difficultyType;

        public string Id => _id;
        public string Name => _name;
        public string GridId => _gridId;
        public string ImageKey => _imageKey;
        public int Difficulty => _difficulty;
        public string[] CellActionIds => _cellActionIds;
        public int MaxDuration => _maxDuration;
        public DifficultyType DifficultyType => Enum.TryParse(_difficultyType, out DifficultyType difficultyType) ? difficultyType : DifficultyType.Easy;

        public static DifficultyType GetCurrentDifficultyType()
        {
            var str = PlayerPrefs.GetString("JigsawLevelData_CurrentDifficultyType", "");
            return Enum.TryParse(str, out DifficultyType difficultyType) ? difficultyType : DifficultyType.Easy;
        }

        public static void SetCurrentDifficultyType(DifficultyType difficultyType)
        {
            PlayerPrefs.SetString("JigsawLevelData_CurrentDifficultyType", difficultyType.ToString());
        }
    }
}