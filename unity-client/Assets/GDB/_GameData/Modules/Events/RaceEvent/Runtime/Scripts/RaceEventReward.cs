using System;
using UniTx.Runtime;
using UnityEngine;

public class RaceEventReward : MonoBehaviour
{
    public void GiveReward()
    {
        UniStatics.LogInfo("Reward given");
    }
}