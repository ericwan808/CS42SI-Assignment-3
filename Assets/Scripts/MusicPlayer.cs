using UnityEngine;

/// <summary>
/// Plays the background music and keeps it playing across scene changes.
/// Put one in every scene that could be the first one opened; only the
/// first copy survives, so the song never restarts or doubles up.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // Music is already playing from an earlier scene.
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        AudioSource source = GetComponent<AudioSource>();
        if (!source.isPlaying)
            source.Play();
    }
}
