using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TestTimer : MonoBehaviour
{
    public TextMeshProUGUI _timerText;
    private ITimeService timeService;
    private void Awake()
    {
        timeService = new RealTimeService("Test", () =>
        {
            Destroy(gameObject);
        });

    }
    [Button]
    public void StartTimer()
    {
        timeService.StartTimer(20);
    }
    void Update()
    {
        timeService.Update();
        _timerText.text = timeService.GetFormattedTime();
    }
    void OnApplicationQuit()
    {
        timeService.SaveTimer();
    }
}
