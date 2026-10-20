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
        if (owned.Count == 0) return;

        // Fit a grid of cells inside the click area.
        Vector2 area = clickArea != null ? clickArea.size : Vector2.one * 2f;
        Vector2 center = clickArea != null ? clickArea.offset : Vector2.zero;
        int columns = Mathf.Min(owned.Count, maxColumns);
        int rows = Mathf.CeilToInt(owned.Count / (float)columns);
        float cell = Mathf.Min(area.x / columns, area.y / rows);
        cellSize = cell;

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

    private GameObject CreateDieDisplay((int sides, int num, Sprite sprite) die, Vector2 position, float cell)
    {
        GameObject display = new GameObject($"D{die.sides} Display");
        display.transform.SetParent(transform, false);
        display.transform.localPosition = position;

        // Die sprite, scaled to fill most of its cell.
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(display.transform, false);
        SpriteRenderer iconRenderer = icon.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = die.sprite;
        iconRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        iconRenderer.sortingOrder = spriteRenderer.sortingOrder;
        if (die.sprite != null)
        {
            Vector3 size = die.sprite.bounds.size;
            float scale = cell * 0.7f / Mathf.Max(size.x, size.y);
            icon.transform.localScale = Vector3.one * scale;
        }
        diceIcons.Add(icon.transform);

        // Count label in the bottom-right corner of the cell.
        GameObject label = new GameObject("Count");
        label.transform.SetParent(display.transform, false);
        TextMeshPro text = label.AddComponent<TextMeshPro>();
        text.text = $"x{die.num}";
        text.alignment = TextAlignmentOptions.BottomRight;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0.1f;
        text.fontSizeMax = 100f;
        text.color = Color.white;
        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;
        text.sortingLayerID = spriteRenderer.sortingLayerID;
        text.sortingOrder = spriteRenderer.sortingOrder + 1;
        text.rectTransform.sizeDelta = new Vector2(cell * 0.45f, cell * 0.3f);
        text.rectTransform.localPosition = new Vector2(cell * 0.25f, -cell * 0.33f);

        return display;
    }

    /// <summary>
    /// This function is called when the mouse button clicks
    /// on this object.
    /// </summary>
    private void OnMouseDown()
    {
        //Debug.Log("clicked");
        Clicks += bag.Roll();

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
