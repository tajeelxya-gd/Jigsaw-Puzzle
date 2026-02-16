using System;
using UnityEngine;
using UnityEngine.Events;

public class OnShowGreatRaceInfoCommand : PopUpExecutionCommand
{
    public OnShowGreatRaceInfoCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
