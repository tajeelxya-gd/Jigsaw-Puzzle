using System;
using UnityEngine;
public class PuzzleManiaParent : MonoBehaviour
{
    private void OnEnable()
    {
        SignalBus.Publish(new PuzzleManiaPanelOpenSignal());
    }
}
public class PuzzleManiaPanelOpenSignal:ISignal{}
