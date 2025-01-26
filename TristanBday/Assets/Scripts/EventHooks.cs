using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class EventHooks
{
    /// <summary>
    /// Invoked when a hold is clicked to release any joints attached
    /// </summary>
    public static EventHook HoldReleased = new EventHook(nameof(HoldReleased));
}
