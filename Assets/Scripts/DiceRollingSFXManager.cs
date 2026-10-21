using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRollingSFXManager : MonoBehaviour
{
    // Eric W
    // array of audio source objects to play
    [SerializeField] private AudioSource[] sources = new AudioSource[4];

    // play audio to simulate numDice being rolled
    public void playDice(int numDice)
    {
        // capped at 20
        numDice = numDice <= 20 ? numDice : 20;
        List<int> selectedAudio = selectRandomAudios(numDice);

        foreach (int i in selectedAudio)
        {
            // source 1 sounds like one dice
            // source 2 sounds like two dice
            // sources 3 and 4 sound like many dice
            StartCoroutine(PlayDelayed(sources[i - 1], Random.Range(0f, 0.1f)));
        }
    } 

    // picks random numbers that sum up to numDice
    private List<int> selectRandomAudios(int numDice)
    {
        List<int> selectedAudio = new List<int>();
        int remaining = numDice;

        while (remaining > 4)
        {
            int i = Random.Range(1, 5);
            selectedAudio.Add(i);
            remaining -= i;
        }

        if (remaining > 0)
            selectedAudio.Add(remaining);

        return selectedAudio;
    }

    // Coroutine to delay audio by random amount, created with the help of Claude
    private IEnumerator PlayDelayed(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        src.PlayOneShot(src.clip);
    }


}
