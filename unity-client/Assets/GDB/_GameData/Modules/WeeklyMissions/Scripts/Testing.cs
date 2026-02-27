using Sirenix.OdinInspector;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [Button]
    public void CompleteMission(MissionType type, int targetAmount)
    {
        SignalBus.Publish(new OnPlayerDidActionSignal()
        {
            MissionType = type,
            Amount = targetAmount
        });
    }
}
