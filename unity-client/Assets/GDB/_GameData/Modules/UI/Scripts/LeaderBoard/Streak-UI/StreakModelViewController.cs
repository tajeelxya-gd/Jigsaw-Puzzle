using System;
using UnityEngine;

public class StreakModelViewController : MonoBehaviour
{

    [SerializeField] StreakModelView[] _streakModelView;
    [SerializeField] private bool updateOnStart = true;
    private StreakModelView _currentStreakModelView;
    private GameData _gameData;

    private void Awake()
    {
        _gameData = GlobalService.GameData;
    }

    private void Start()
    {
        if (!updateOnStart) return;
        UpdateStreaks();
    }

    public void UpdateStreaks()
    {
        if(_gameData == null)
            _gameData = GlobalService.GameData;

        _gameData.Data.CurrentWinStreakLevel = Mathf.Clamp( _gameData.Data.CurrentWinStreakLevel,0, _streakModelView.Length - 1);
        _gameData.Save();
        for (int i = 0; i < _streakModelView.Length; i++)
        {
            _streakModelView[i].OnSelected(i == _gameData.Data.CurrentWinStreakLevel);
            if (i == _gameData.Data.CurrentWinStreakLevel)
                _currentStreakModelView = _streakModelView[i];
        }
    }

    public int GetCurrentStreakRewardMultiplier()
    {
        if (_currentStreakModelView != null) return _currentStreakModelView.RewardMultiplier;
        return 1;
    }
}
