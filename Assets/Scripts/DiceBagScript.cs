using System.Collections.Generic;
using UnityEngine;

public class DiceBagScript : MonoBehaviour
{

    private Dictionary<int, int> dice;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         dice = new Dictionary<int, int>{ { 1, 1 } };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Roll()
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
    public void AddDice(int val)
    {
        if (val < 1) return;

        if (dice.ContainsKey(val))
            dice[val] += 1;
        else
            dice[val] = 1;
    }
}
