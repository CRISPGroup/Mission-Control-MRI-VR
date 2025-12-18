using UnityEngine;

/// <summary>
/// Plays looping background sounds or ambient audio through a configured <see cref="AudioSource"/>.
/// </summary>
/// <remarks>
/// This script allows simple playback of looping audio clips, such as ambient background sounds,
/// machine hums, or environmental effects.  
/// It assumes that the <see cref="AudioSource"/> component is already configured in the Inspector.
/// </remarks>
public class PlaySoundLoopSimple : MonoBehaviour
{
    [Tooltip("The AudioSource component used to play the sound.")]
    public AudioSource audioSource;

    /// <summary>
    /// Initializes the AudioSource and ensures that it loops continuously.
    /// </summary>
    private void Start()
    {
        // Ensure the AudioSource is configured for looping
        if (audioSource == null)
        {
            Debug.LogError("AudioSource reference is missing on " + gameObject.name);
            return;
        }

        audioSource.loop = true;
    }

    /// <summary>
    /// Plays a specified audio clip in a continuous loop.
    /// </summary>
    /// <param name="clip">The audio clip to be played.</param>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("No AudioClip assigned!");
            return;
        }

        // Assign the clip to the AudioSource and play
        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// Stops the currently playing sound if the AudioSource is active.
    /// </summary>
    public void StopSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
