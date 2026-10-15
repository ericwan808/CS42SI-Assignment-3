using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single die. Create one with the number of sides you want: new Die(6), new Die(20), etc.
/// </summary>
public class Die
{
    public int Sides;

    public Die(int sides)
    {
        Sides = sides;
    }

    // Returns a random number from 1 to Sides (inclusive).
    public int Roll()
    {
        return Random.Range(1, Sides + 1);
    }
}

/// <summary>
/// Global game data. Static, so it is NOT attached to any GameObject.
/// Access anything here from any script with: GameData.<Name>
/// </summary>
public static class GameData
{
    // ---------------------------------------------------------------
    // SCORE: the player's current score for this run.
    //
    // Usage from another script:
    //   GameData.Score += 10;                       // add points
    //   Debug.Log("Score: " + GameData.Score);      // read it
    //   scoreText.text = "Score: " + GameData.Score;
    // ---------------------------------------------------------------
    public static int Score = 0;

    // ---------------------------------------------------------------
    // HIGHSCORE: the best score ever reached. Saved with PlayerPrefs,
    // so it survives closing the game (unlike Score and DiceBag).
    //
    // Usage from another script:
    //   if (GameData.Score > GameData.HighScore)
    //       GameData.HighScore = GameData.Score;    // set a new record
    //
    //   Debug.Log("Best: " + GameData.HighScore);   // read it
    // ---------------------------------------------------------------
    public static int HighScore
    {
        get { return PlayerPrefs.GetInt("HighScore", 0); }
        set
        {
            PlayerPrefs.SetInt("HighScore", value);
            PlayerPrefs.Save();
        }
    }

    // ---------------------------------------------------------------
    // DICEBAG: a list holding every die the player owns.
    //
    // Usage from another script:
    //   GameData.DiceBag.Add(new Die(6));           // pick up a d6
    //   GameData.DiceBag.Add(new Die(20));          // pick up a d20
    //
    //   // Roll every die in the bag and total the results:
    //   int total = 0;
    //   foreach (Die die in GameData.DiceBag)
    //       total += die.Roll();
    //
    //   Debug.Log("Dice owned: " + GameData.DiceBag.Count);
    //   GameData.DiceBag.RemoveAt(0);               // remove the first die
    // ---------------------------------------------------------------
    public static List<Die> DiceBag = new List<Die>();

    // ---------------------------------------------------------------
    // Call this when a new game starts (e.g. in Start() of your first
    // scene): GameData.Reset();
    // HighScore is intentionally NOT reset.
    // ---------------------------------------------------------------
    public static void Reset()
    {
        Score = 0;
        DiceBag.Clear();
    }
}