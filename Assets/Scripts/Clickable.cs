using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Clickable : MonoBehaviour
{
    public DiceBagScript bag;

    // This variable is marked as `static`, which means it is
    // accessible by any script, anywhere! They can just write
    // Clickable.Clicks to read/write to this value.
    //
    // See an example in `ClickTracker.cs`, which is on a UI
    // object titled `Tracker Text (TMP)`.
    public static double Clicks = 0;

    /// <summary>
    /// This function is called when the mouse button clicks
    /// on this object.
    /// </summary>
    private void OnMouseDown()
    {
        Clicks += bag.Roll();
    }

}
