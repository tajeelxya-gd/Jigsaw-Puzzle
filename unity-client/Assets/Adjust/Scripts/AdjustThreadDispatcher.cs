using System;
using System.Collections.Generic;
using Monetization.Runtime.Utilities;
using UnityEngine;

public class AdjustThreadDispatcher
{
    public static void RunOnMainThread(Action action)
    {
        if (action == null)
        {
            return;
        }

        ThreadDispatcher.Enqueue(action);
    }
}