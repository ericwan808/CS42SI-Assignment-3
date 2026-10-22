using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    // The total of the most recent roll, or null before the first click.
    public static int? LastRoll = null;

    [Header("Debug")]
    [SerializeField] private double debugAddClicks = 1000000;

    // In Play mode, use the ⋮ menu on this component and pick "Add Debug Clicks".
    [ContextMenu("Add Debug Clicks")]
    private void AddDebugClicks() => Clicks += debugAddClicks;

    // Max dice per row before wrapping onto a new row.
    [SerializeField] private int maxColumns = 3;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D clickArea;

    // One child object per die type currently shown on the clicker.
    private readonly List<GameObject> diceDisplays = new List<GameObject>();

    // The die sprites inside diceDisplays, which shake when clicked.
    private readonly List<Transform> diceIcons = new List<Transform>();

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeDistance = 0.08f; // fraction of a die's cell size
    [SerializeField] private float shakeAngle = 20f;      // max tilt in degrees

    private float cellSize = 1f;
    private Coroutine shakeRoutine;

    [SerializeField] private DiceRollingSFXManager audioManager;

    [Header("Bag")]
    [SerializeField] private float bagPadding = 0.15f;     // leather around the dice, as a fraction of a die's cell
    [SerializeField] private float bagTopSize = 0.45f;     // how tall the cinched top is, relative to the bag's size
    [SerializeField] private int bagPixelsPerUnit = 256;   // texture resolution of the bag
    [SerializeField, Range(0f, 1f)] private float bagOpacity = 0.6f;

    // Extra leather below the bottom row on two-row bags, in cells, so its count labels clear the seam.
    private const float TwoRowLabelRoom = 0.12f;

    private SpriteRenderer bagRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        clickArea = GetComponent<BoxCollider2D>();

        // The individual dice are drawn by child objects instead.
        spriteRenderer.enabled = false;
    }

    private void Start()
    {
        RefreshDice();
    }

    /// <summary>
    /// Rebuilds the clicker so it shows one of each die type the
    /// player owns, with a label showing how many of that die they have.
    /// </summary>
    public void RefreshDice()
    {
        foreach (GameObject display in diceDisplays)
        {
            Destroy(display);
        }
        diceDisplays.Clear();
        diceIcons.Clear();

        var owned = bag.OwnedDice();
        if (owned.Count == 0)
        {
            if (bagRenderer != null) bagRenderer.enabled = false;
            return;
        }

        // Fit a grid of cells inside the click area.
        Vector2 area = clickArea != null ? clickArea.size : Vector2.one * 2f;
        Vector2 center = clickArea != null ? clickArea.offset : Vector2.zero;
        int columns = Mathf.Min(owned.Count, maxColumns);
        int rows = Mathf.CeilToInt(owned.Count / (float)columns);

        // The dice sit in the bag's body, with some leather around them; the
        // cinched top sits above. Measured in cells, then scaled so the whole
        // bag fits inside the click area.
        float bodyCellsWide = columns + 2f * bagPadding;
        // Two-row bags are tall enough that the normal rounded corners would cut into
        // the bottom row, so they get tighter corners and a little extra room below.
        bool twoRows = rows > 1;
        float labelRoom = twoRows ? TwoRowLabelRoom : 0f;
        float bodyCellsTall = rows + 2f * bagPadding + labelRoom;
        float topCells = bagTopSize * Mathf.Sqrt(bodyCellsWide * bodyCellsTall);
        float cell = Mathf.Min(area.x / bodyCellsWide, area.y / (bodyCellsTall + topCells));
        cellSize = cell;

        Vector2 bagSize = new Vector2(bodyCellsWide, bodyCellsTall + topCells) * cell;
        float bodyHeight = bodyCellsTall * cell;
        UpdateBag(center, bagSize, bodyHeight, cell, twoRows);

        // Center the dice in the bag's body rather than the whole bag,
        // nudged up to leave the extra room below the bottom row.
        center.y += (bodyHeight - bagSize.y) / 2f + labelRoom * cell / 2f;

        for (int i = 0; i < owned.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            // Center a partially filled last row.
            int inThisRow = Mathf.Min(columns, owned.Count - row * columns);
            float x = (col - (inThisRow - 1) / 2f) * cell;
            float y = ((rows - 1) / 2f - row) * cell;

            diceDisplays.Add(CreateDieDisplay(owned[i], center + new Vector2(x, y), cell));
        }
    }

    // Redraws the leather bag to fit the dice, centered at position.
    private void UpdateBag(Vector2 position, Vector2 size, float bodyHeight, float cell, bool tightCorners)
    {
        if (bagRenderer == null)
        {
            GameObject go = new GameObject("Dice Bag");
            go.transform.SetParent(transform, false);
            bagRenderer = go.AddComponent<SpriteRenderer>();
            bagRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            bagRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // behind the dice
        }

        // Free the previous drawing before replacing it.
        if (bagRenderer.sprite != null)
        {
            Destroy(bagRenderer.sprite.texture);
            Destroy(bagRenderer.sprite);
        }

        int width = Mathf.Max(8, Mathf.RoundToInt(size.x * bagPixelsPerUnit));
        int height = Mathf.Max(8, Mathf.RoundToInt(size.y * bagPixelsPerUnit));
        int bodyPixels = Mathf.RoundToInt(bodyHeight * bagPixelsPerUnit);
        float detailScale = cell * bagPixelsPerUnit / 150f;

        Texture2D tex = LeatherBagArt.Draw(width, height, bodyPixels, detailScale, tightCorners);
        bagRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), bagPixelsPerUnit);
        bagRenderer.color = new Color(1f, 1f, 1f, bagOpacity);
        bagRenderer.transform.localPosition = position;
        bagRenderer.enabled = true;
    }

    private GameObject CreateDieDisplay((int sides, int num, Sprite sprite) die, Vector2 position, float cell)
    {
        GameObject display = new GameObject($"D{die.sides} Display");
        display.transform.SetParent(transform, false);
        // Shifted up a little so the die sits in the upper part of its cell,
        // leaving room for the count below. (The die itself stays at the
        // display's origin, which is where the shake animation returns it.)
        display.transform.localPosition = position + new Vector2(0f, cell * 0.08f);

        // Die sprite.
        float iconSize = cell * 0.62f;
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(display.transform, false);
        SpriteRenderer iconRenderer = icon.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = die.sprite;
        iconRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        iconRenderer.sortingOrder = spriteRenderer.sortingOrder;
        if (die.sprite != null)
        {
            Vector3 size = die.sprite.bounds.size;
            float scale = iconSize / Mathf.Max(size.x, size.y);
            icon.transform.localScale = Vector3.one * scale;
        }
        diceIcons.Add(icon.transform);

        // Count label centered under the die, clear of the bag's rounded corners.
        GameObject label = new GameObject("Count");
        label.transform.SetParent(display.transform, false);
        TextMeshPro text = label.AddComponent<TextMeshPro>();
        text.text = $"x{die.num}";
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0.1f;
        text.fontSizeMax = 100f;
        text.color = Color.white;
        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;
        text.sortingLayerID = spriteRenderer.sortingLayerID;
        text.sortingOrder = spriteRenderer.sortingOrder + 1;
        text.rectTransform.sizeDelta = new Vector2(cell * 0.6f, cell * 0.22f);
        text.rectTransform.localPosition = new Vector2(0f, -cell * 0.44f);

        return display;
    }

    /// <summary>
    /// This function is called when the mouse button clicks
    /// on this object.
    /// </summary>
    private void OnMouseDown()
    {
        int roll = bag.Roll();
        LastRoll = roll;
        Clicks += roll;
        audioManager.playDice(bag.getTotalDice());

        // Restart the shake if the player clicks again mid-shake.
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(ShakeDice());
    }

    /// <summary>
    /// Jiggles each die independently, fading out over shakeDuration,
    /// then puts every die back where it started.
    /// </summary>
    private IEnumerator ShakeDice()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float strength = 1f - elapsed / shakeDuration;
            foreach (Transform icon in diceIcons)
            {
                if (icon == null) continue;
                icon.localPosition = Random.insideUnitCircle * (shakeDistance * cellSize * strength);
                icon.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-shakeAngle, shakeAngle) * strength);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (Transform icon in diceIcons)
        {
            if (icon == null) continue;
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
        }
        shakeRoutine = null;
    }

}
