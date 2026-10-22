using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceBagScript : MonoBehaviour
{
    public Sprite D1;
    public Sprite D4;
    public Sprite D6;
    public Sprite D8;
    public Sprite D12;
    public Sprite D20;

    public Clickable clicker;

    // Fired whenever a die is added, so other scripts can react to the new bag.
    public event System.Action DiceChanged;

    private Dictionary<int, (int num, Sprite sprite)> dice;

    // Every colored version of each die type, keyed by number of sides.
    // Loaded from the sprite sheets in Assets/Sprites/Resources/DiceColors.
    private Dictionary<int, List<Sprite>> colorVariants;

    // Every dice sheet is sliced the same way; this maps slice index -> sides.
    private static readonly Dictionary<int, int> sliceToSides = new Dictionary<int, int> {
        { 0, 20 },
        { 1, 12 },
        { 4, 8 },
        { 5, 6 },
        { 6, 4 }};

    // EW
    // aparently Awake runs before Start,
    // so sprites needs to be set here before ButtonSpawner tries to access it on Start
    void Awake()
    {
        dice = new Dictionary<int, (int num, Sprite sprite)> {
            { 1, (1, D1)},
            { 4, (0, D4)},
            { 6, (0, D6)},
            { 8, (0, D8)},
            { 12, (0, D12)},
            { 20, (0, D20)}};

        LoadColorVariants();
    }

    private void LoadColorVariants()
    {
        colorVariants = new Dictionary<int, List<Sprite>>();
        foreach (Sprite sprite in Resources.LoadAll<Sprite>("DiceColors"))
        {
            // Slices are named like "small_dice_redyellow_4"; the number is the slice index.
            string suffix = sprite.name.Substring(sprite.name.LastIndexOf('_') + 1);
            if (!int.TryParse(suffix, out int slice) || !sliceToSides.TryGetValue(slice, out int sides))
                continue;

            if (!colorVariants.ContainsKey(sides))
                colorVariants[sides] = new List<Sprite>();
            colorVariants[sides].Add(sprite);
        }
    }

    // EW
    // rolls all dice in bag and returns sum
    public int Roll()
    {
        int total = 0;
        foreach (KeyValuePair<int, (int, Sprite)> pair in dice)
        {
            int value = pair.Key;
            int amount = pair.Value.Item1;
            for (int i = 0; i < amount; i++)
            {
                total += Random.Range(1, value + 1);
            }
        }
        return total;
    }
    
    // Returns how many dice with the given number of sides are in the bag.
    public int CountOf(int sides)
    {
        return dice.TryGetValue(sides, out var entry) ? entry.num : 0;
    }

    // Adds one die with the given value to the bag.
    // Values below 1 are ignored.
    //   DiceBag.AddDice(6);   // add a d6
    //   DiceBag.AddDice(20);  // add a d20
    public void AddDice(int val)
    {
        if (val < 1) return;

        if (dice.ContainsKey(val))
        {
            // A die type keeps its default color on its first purchase; buying
            // another one gives just this type a new random color.
            Sprite sprite = dice[val].sprite;
            if (dice[val].num > 0 && colorVariants.TryGetValue(val, out List<Sprite> variants) && variants.Count > 0)
            {
                sprite = variants[Random.Range(0, variants.Count)];
            }
            dice[val] = (dice[val].num + 1, sprite);

            // Rebuild the clicker so it shows every owned die type and its count.
            clicker.RefreshDice();

            DiceChanged?.Invoke();
        }
    }

    // Returns every die type the player owns at least one of, smallest first.
    public List<(int sides, int num, Sprite sprite)> OwnedDice()
    {
        var owned = new List<(int sides, int num, Sprite sprite)>();
        foreach (KeyValuePair<int, (int num, Sprite sprite)> pair in dice)
        {
            if (pair.Value.num > 0)
            {
                owned.Add((pair.Key, pair.Value.num, pair.Value.sprite));
            }
        }
        owned.Sort((a, b) => a.sides.CompareTo(b.sides));
        return owned;
    }

    // EW
    // gets the sprite of a particular dice value
    public Sprite getSprite(int val)
    {
        if (dice.ContainsKey(val))
        {
            return dice[val].sprite;
        } else
        {
            return dice[0].sprite;
        }
    }

    // EW
    // gets total number of dice in the bag
    public int getTotalDice()
    {
        int sum = 0;
        foreach (KeyValuePair<int, (int num, Sprite sprite)> pair in dice)
        {
            sum += pair.Value.num;
        }

        return sum;
    }
}
