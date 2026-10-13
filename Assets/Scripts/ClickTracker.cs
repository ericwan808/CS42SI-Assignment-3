using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickTracker : MonoBehaviour
{

    TextMeshProUGUI _trackingText;

    /// <summary>
    /// Awake is run before the Start() method, usually used for
    /// initializing values.
    /// </summary>
    private void Awake()
    {
        _trackingText = GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        // See how many clicks the player has, stored in the static
        // variable `Clickable.Clicks`.
        _trackingText.text = "Clicks: " + Clickable.Clicks;
    }

}
