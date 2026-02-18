using System;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class UserSavedData : IUserSavedData
    {
        [SerializeField] private string _id;
        [SerializeField] private long _modifiedTimestamp;
        [SerializeField] private JigsawLevelState _currentLevelState;

        public string Id => _id;
        public long ModifiedTimestamp { get => _modifiedTimestamp; set => _modifiedTimestamp = value; }
        public int CurrentLevel
        {
            get => GlobalService.GameData.Data.LevelNumber;
            set
            {
                GlobalService.GameData.Data.LevelNumber = value;
                GlobalService.GameData.Save();
            }
        }
        public JigsawLevelState CurrentLevelState { get => _currentLevelState; set => _currentLevelState = value; }
    }
}