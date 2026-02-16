using System;
using UnityEngine;
using UnityEngine.Events;

public class OfferPanelsShowCommand : PopUpExecutionCommand
{
    public OfferPanelsShowCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
