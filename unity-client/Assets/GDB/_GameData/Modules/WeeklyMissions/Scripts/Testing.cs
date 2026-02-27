using Sirenix.OdinInspector;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [Button]
    public void Get2XCoins()
    {
        SignalBus.Publish(new OnPlayerDidActionSignal()
        {
            MissionType = MissionType.Get2XCoins,
            Amount = 1
        });
    }
    [Button]
    public void Login()
    {
        SignalBus.Publish(new OnPlayerDidActionSignal()
        {
            MissionType = MissionType.Login,
            Amount = 1
        });
    }
    [Button]

    public void WinStreak()
    {
        SignalBus.Publish(new OnPlayerDidActionSignal()
        {
            MissionType = MissionType.WinStreak,
            Amount = 1
        });
    }
    [Button]
    public void UseMagnet()
    {
        SignalBus.Publish(new OnPlayerDidActionSignal()
        {
            MissionType = MissionType.UseMagnet,
            Amount = 1
        });
    }
    [Button]
    public void UseEye()
    {
        SignalBus.Publish(new OnPlayerDidActionSignal()
        {
            MissionType = MissionType.UseEye,
            Amount = 1
        });
    }
}
