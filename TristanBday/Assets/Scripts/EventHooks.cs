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
    
    /// <summary>
    /// Invoked when the last hold has been reached
    /// </summary>
    public static EventHook LastHoldReached = new EventHook(nameof(LastHoldReached));
    
    /// <summary>
    /// Invoked to show caption text
    /// </summary>
    public static EventHook ShowCaptionText = new EventHook(nameof(ShowCaptionText));
}
