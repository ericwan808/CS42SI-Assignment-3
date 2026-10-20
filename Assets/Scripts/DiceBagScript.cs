using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceBagScript : MonoBehaviour
{

    public Sprite largestDice;
    public Sprite D1;
    public Sprite D4;
    public Sprite D6;
    public Sprite D8;
    public Sprite D12;
    public Sprite D20;

    public Clickable clicker;

    private Dictionary<int, (int num, Sprite sprite)> dice;


    void Awake()
    {
        dice = new Dictionary<int, (int num, Sprite sprite)> {
            { 1, (1, D1)},
            { 4, (0, D4)},
            { 6, (0, D6)},
            { 8, (0, D8)},
            { 12, (0, D12)},
            { 20, (0, D20)}};
    }

    public int Roll()
    {
        int total = 0;
        foreach (KeyValuePair<int, (int, Sprite)> pair in dice)
        {
            int value = pair.Key;    // sides on the die
            int amount = pair.Value.Item1; // how many of them
            for (int i = 0; i < amount; i++)
            {
                total += Random.Range(1, value + 1); // int upper bound is exclusive
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
            if (dice[val].num == 0)
            {
                largestDice = dice[val].sprite;
                clicker.updateSprite(largestDice);
            }
            dice[val] = (dice[val].num + 1, dice[val].sprite);

            //Debug.Log(dice[val]);
        }
    }

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
}
