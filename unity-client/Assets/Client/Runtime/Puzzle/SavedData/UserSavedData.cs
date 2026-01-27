using System;
using UniTx.Runtime.Clock;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public sealed class UserSavedData : IUserSavedData
    {
        [SerializeField] private string _id;
        [SerializeField] private long _modifiedTimestamp;
        [SerializeField] private int _currentLevel;
        [SerializeField] private JigsawLevelState _currentLevelState;

        public string Id => _id;
        public long ModifiedTimestamp { get => _modifiedTimestamp; set => _modifiedTimestamp = value; }
        public int CurrentLevel { get => _currentLevel; set => _currentLevel = value; }
        public JigsawLevelState CurrentLevelState { get => _currentLevelState; set => _currentLevelState = value; }
    }
}