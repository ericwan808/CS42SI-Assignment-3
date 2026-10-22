using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "Show roll distribution" checkbox and panel in the scene.
/// The panel plots the chance of every possible total for the dice
/// currently in the bag, from the minimum to the maximum roll.
///
/// The UI objects live in the scene (create them with
/// Tools > Build Roll Distribution UI); this script only fills them in.
/// </summary>
public class RollDistributionUI : MonoBehaviour
{
    public DiceBagScript bag;

    // The clicker's collider; the panel is narrowed so it never reaches this area.
    public Collider2D diceArea;

    [Header("Scene UI")]
    public Toggle toggle;
    public RectTransform panel; // keep its pivot on the left edge
    public RawImage plotImage;
    public TextMeshProUGUI minLabel;
    public TextMeshProUGUI maxLabel;
    public TextMeshProUGUI peakLabel;

    [Header("Plot")]
    public Color32 barColor = new Color32(255, 255, 255, 255);
    public Color32 axisColor = new Color32(30, 30, 30, 255);
    // Gap between the panel and the dice, in canvas units.
    public float diceMargin = 20f;

    private Canvas canvas;
    private Texture2D plotTexture;
    private float designedWidth;

    // distribution[i] is the chance of rolling a total of minRoll + i.
    private double[] distribution;
    private int minRoll;

    void Start()
    {
        canvas = panel.GetComponentInParent<Canvas>().rootCanvas;
        designedWidth = panel.sizeDelta.x;

        // The plot is drawn into a small texture and stretched to fill the Plot object.
        plotTexture = new Texture2D(256, 128, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        plotImage.texture = plotTexture;

        toggle.onValueChanged.AddListener(SetPanelVisible);
        SetPanelVisible(toggle.isOn);

        bag.DiceChanged += Recalculate;
        Recalculate();
    }

    void OnDestroy()
    {
        if (bag != null)
            bag.DiceChanged -= Recalculate;
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(SetPanelVisible);
    }

    void LateUpdate()
    {
        if (panel.gameObject.activeSelf)
            KeepClearOfDice();
    }

    private void SetPanelVisible(bool visible)
    {
        panel.gameObject.SetActive(visible);
        if (visible) KeepClearOfDice();
    }

    // Uses the width set in the editor, but narrows the panel on screens
    // where that would run into the dice.
    private void KeepClearOfDice()
    {
        float width = designedWidth;
        Camera cam = Camera.main;
        if (cam != null && diceArea != null)
        {
            var corners = new Vector3[4];
            panel.GetWorldCorners(corners); // screen pixels on an overlay canvas
            float panelLeft = corners[0].x;
            float diceLeft = cam.WorldToScreenPoint(diceArea.bounds.min).x;
            float room = (diceLeft - panelLeft) / canvas.scaleFactor - diceMargin;
            width = Mathf.Clamp(room, 0f, designedWidth);
        }
        panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
    }

    // ---------- Distribution ----------

    private void Recalculate()
    {
        // Start with "a total of 0, guaranteed", then add each die one at a time.
        var dist = new List<double> { 1.0 };
        minRoll = 0;

        foreach (var die in bag.OwnedDice())
        {
            for (int n = 0; n < die.num; n++)
            {
                dist = AddDie(dist, die.sides);
                minRoll += 1;
            }
        }

        distribution = dist.ToArray();
        DrawPlot();
        UpdateLabels();
    }

    // Convolves the current distribution with one fair die of the given sides.
    // Uses a running window sum so it stays fast with many dice.
    private static List<double> AddDie(List<double> dist, int sides)
    {
        int length = dist.Count + sides - 1;
        var result = new List<double>(length);
        double window = 0;
        for (int k = 0; k < length; k++)
        {
            if (k < dist.Count) window += dist[k];
            if (k - sides >= 0) window -= dist[k - sides];
            result.Add(window / sides);
        }
        return result;
    }

    private void DrawPlot()
    {
        int width = plotTexture.width;
        int height = plotTexture.height;
        int count = distribution.Length;
        var pixels = new Color32[width * height]; // starts fully transparent

        double peak = 0;
        foreach (double p in distribution)
            peak = System.Math.Max(peak, p);

        // Leave a 1px gap between bars when they are wide enough to see it.
        bool gaps = count * 3 <= width;

        for (int x = 0; x < width; x++)
        {
            int i = Mathf.Min(count - 1, x * count / width);
            bool lastColumnOfBar = (x + 1) * count / width > i;
            if (gaps && lastColumnOfBar && x != width - 1) continue;

            int barHeight = peak > 0 ? Mathf.CeilToInt((float)(distribution[i] / peak) * (height - 1)) : 0;
            for (int y = 1; y <= barHeight && y < height; y++)
                pixels[y * width + x] = barColor;
        }

        // Baseline along the bottom.
        for (int x = 0; x < width; x++)
            pixels[x] = axisColor;

        plotTexture.SetPixels32(pixels);
        plotTexture.Apply();
    }

    private void UpdateLabels()
    {
        int mode = 0;
        for (int i = 1; i < distribution.Length; i++)
            if (distribution[i] > distribution[mode]) mode = i;

        minLabel.text = $"Min {minRoll}";
        maxLabel.text = $"Max {minRoll + distribution.Length - 1}";
        peakLabel.text = $"Most likely {minRoll + mode} ({distribution[mode]:P1})";
    }
}
