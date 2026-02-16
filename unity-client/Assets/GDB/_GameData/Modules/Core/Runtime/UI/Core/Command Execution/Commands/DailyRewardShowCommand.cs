using System;
using UnityEngine;
using UnityEngine.Events;

public class DailyRewardShowCommand : PopUpExecutionCommand
{
    public DailyRewardShowCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
