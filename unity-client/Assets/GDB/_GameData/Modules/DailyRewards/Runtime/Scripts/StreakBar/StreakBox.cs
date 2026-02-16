using UnityEngine;
using UnityEngine.UI;

public class StreakBox : MonoBehaviour
{
    [SerializeField] private int _rewardAtDay; 
    [SerializeField] private WeeklyRewardType[] _rewards;
    [SerializeField] private Image _box;
    [SerializeField] private Sprite _boxOpen;
    [SerializeField] private Sprite _boxClosed;
    [SerializeField] private GameObject _greenCircle;
     

    private string PrefKey => $"StreakBox_{_rewardAtDay}";

    private void OnEnable()
    {
        bool isOpened = PlayerPrefs.GetInt(PrefKey, 0) == 1;

        if (isOpened)
            OpenVisual();
    }

    public int GetDay()
    {
        return _rewardAtDay;
    }

    // public void GiveReward()
    // {
    //     if (PlayerPrefs.GetInt(PrefKey, 0) == 1)
    //         return;
    //
    //     PlayerPrefs.SetInt(PrefKey, 1);
    //     PlayerPrefs.Save();
    //
    //     OpenVisual();
    //
    //     for (int i = 0; i < _rewards.Length; i++)
    //     {
    //         // Give reward here
    //     }
    // }

    public void OpenVisual()
    {
        _box.sprite = _boxOpen;
        _greenCircle.SetActive(true);
    }

    public void ResetBox()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        _greenCircle.SetActive(false);
        _box.sprite = _boxClosed;
    }
}