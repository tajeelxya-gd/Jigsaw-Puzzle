using System;
using UnityEngine;
using UnityEngine.Events;

public class StartRaceCommand : PopUpExecutionCommand
{
    public StartRaceCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
