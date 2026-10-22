using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickTracker : MonoBehaviour
{
    // The panel behind the text; it is resized to fit the text every frame.
    // If left empty, a sibling named "Score Panel" is used.
    [SerializeField] private RectTransform background;

    // The panel never gets narrower than this, so it doesn't jump around for small numbers.
    [SerializeField] private float minTextWidth = 260f;

    TextMeshProUGUI _trackingText;

    /// <summary>
    /// Awake is run before the Start() method, usually used for
    /// initializing values.
    /// </summary>
    private void Awake()
    {
        _trackingText = GetComponent<TextMeshProUGUI>();
        _trackingText.textWrappingMode = TextWrappingModes.NoWrap;

        if (background == null && transform.parent != null)
            background = transform.parent.Find("Score Panel") as RectTransform;
    }

    public void Update()
    {
        // See how many clicks the player has, stored in the static
        // variable `Clickable.Clicks`.
        // "#,0.##" adds thousands separators, shows at most 2 decimals, and
        // hides tiny floating-point leftovers like 95049.6900000001.
        string lastRolled = Clickable.LastRoll?.ToString("#,0") ?? "-";
        _trackingText.text = "Clicks: " + Clickable.Clicks.ToString("#,0.##")
            + "\n<size=80%>Last rolled: " + lastRolled + "</size>";

        FitToText();
    }

    // Sizes the text box to its contents and the panel to fit around it, with the
    // same padding on the right and bottom as the text already has on the left and top.
    // Both are pinned at their top-left corners, so they grow to the right and down.
    private void FitToText()
    {
        Vector2 textSize = _trackingText.GetPreferredValues();
        textSize.x = Mathf.Max(textSize.x, minTextWidth);
        _trackingText.rectTransform.sizeDelta = textSize;

        if (background != null)
        {
            Vector2 offset = _trackingText.rectTransform.anchoredPosition - background.anchoredPosition;
            Vector2 padding = new Vector2(Mathf.Abs(offset.x), Mathf.Abs(offset.y));
            background.sizeDelta = textSize + padding * 2f;
        }
    }
}
