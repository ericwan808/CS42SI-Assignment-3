using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global dice container. Static, so it is NOT attached to any GameObject.
/// Internally a dictionary of { value, amount }:
///   value  = number of sides on the die (6 = a d6)
///   amount = how many of that die are in the bag
/// Starts with { 1, 1 }: one die with value 1.
/// </summary>
public static class DiceBag
{
    private static Dictionary<int, int> dice = new Dictionary<int, int>
    {
        { 1, 1 }
    };

    // Rolls every die in the bag and returns the total.
    // An empty bag rolls 0.
    //   int result = DiceBag.Roll();
    public static int Roll()
    {
        int total = 0;
        foreach (KeyValuePair<int, int> pair in dice)
        {
            int value = pair.Key;    // sides on the die
            int amount = pair.Value; // how many of them
            for (int i = 0; i < amount; i++)
            {
                total += Random.Range(1, value + 1); // int upper bound is exclusive
            }
        }
        return total;
    }

    // Adds one die with the given value to the bag.
    // Values below 1 are ignored.
    //   DiceBag.AddDice(6);   // add a d6
    //   DiceBag.AddDice(20);  // add a d20
    public static void AddDice(int val)
    {
        if (val < 1) return;

        if (dice.ContainsKey(val))
            dice[val] += 1;
        else
            dice[val] = 1;
    }

    // Removes one die with the given value from the bag.
    // Returns true if a die was removed, false if you didn't own one.
    // When the last die of a value is removed, its entry is deleted.
    //   if (DiceBag.RemoveDice(6)) { /* lost a d6 */ }
    public static bool RemoveDice(int val)
    {
        if (!dice.TryGetValue(val, out int amount)) return false;

        if (amount <= 1)
            dice.Remove(val);
        else
            dice[val] = amount - 1;

        return true;
    }

    // Returns how many dice of the given value are in the bag (0 if none).
    //   int d6s = DiceBag.GetCount(6);
    //   if (DiceBag.GetCount(20) >= 2) { /* has two d20s */ }
    public static int GetCount(int val)
    {
        return dice.TryGetValue(val, out int amount) ? amount : 0;
    }

    // Empties the bag completely (no dice at all).
    //   DiceBag.Clear();
    public static void Clear()
    {
        dice.Clear();
    }

    // Restores the starting state: just { 1, 1 }.
    // Call this when a new game begins.
    //   DiceBag.Reset();
    public static void Reset()
    {
        dice.Clear();
        dice[1] = 1;
    }
}

/// <summary>
/// Global score holder. Static, so it is NOT attached to any GameObject.
/// </summary>
public static class Score
{
    private static int value = 0;

    // Returns the current score.
    //   int current = Score.GetVal();
    //   scoreText.text = "Score: " + Score.GetVal();
    public static int GetVal()
    {
        return value;
    }

    // Sets the score to a new value.
    //   Score.SetVal(100);
    public static void SetVal(int newVal)
    {
        value = newVal;
    }

    // Adds (or subtracts, if negative) points to the score.
    //   Score.AddVal(10);
    //   Score.AddVal(DiceBag.Roll());
    public static void AddVal(int amount)
    {
        value += amount;
    }

    // Sets the score back to 0. Call this when a new game begins.
    //   Score.Reset();
    public static void Reset()
    {
        value = 0;
    }
}}