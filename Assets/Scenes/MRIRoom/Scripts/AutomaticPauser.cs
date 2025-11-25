using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Automatically pauses gameplay and plays audio feedback clips at timed intervals.
/// Handles multilingual audio, text display on pause, and resuming after playback.
/// </summary>
public class AutomaticPauser : MonoBehaviour
{
    [Tooltip("List of English audio clips for feedback.")]
    public List<AudioClip> audioClips;

    [Tooltip("List of French audio clips for feedback.")]
    public List<AudioClip> audioClipsFR;

    [Tooltip("AudioSource used to play feedback clips.")]
    public AudioSource audioSource;

    [Tooltip("Reference to the MenuPauser handling pause state.")]
    public MenuPauser menuPauser;

    [Tooltip("Canvas shown when the pause sequence starts.")]
    public GameObject pauseMovingCanvas;

    [Tooltip("Delay in seconds between automatic feedbacks.")]
    public float delayBetweenFeedbacks = 15f;

    [Tooltip("Event triggered after the last audio clip finishes playing.")]
    public UnityEvent onLastClipFinished;

    private int index = 0;
    private bool isPausing = false;
    private bool isPauseAutomatic = false;
    private bool isStoppedExternally = false;
    private Coroutine pauseRoutine;

    /// <summary>
    /// Sets the index of the next audio clip to play.
    /// </summary>
    public void SetAudioIndex(int index)
    {
        this.index = index;
    }

    /// <summary>
    /// Updates the automatic pause mode based on a UI dropdown value.
    /// </summary>
    /// <param name="dropdown">Dropdown with 0 = manual pauses only, 1 = automatic.</param>
    public void SetIsPauseAutomatic(UnityEngine.UI.Dropdown dropdown)
    {
        if (dropdown.value == 0)
        {
            isPauseAutomatic = false;
        }

        else if (dropdown.value == 1)
        {
            isPauseAutomatic = true;
        }

    }

    /// <summary>
    /// Starts the automatic pause sequence if enabled.
    /// </summary>
    public void StartAutomaticPauser()
    {

        if (isPauseAutomatic && pauseRoutine == null)
        {
            pauseRoutine = StartCoroutine(PauseRoutine());
        }
    }

    /// <summary>
    /// Enables or disables automatic pause directly.
    /// </summary>
    public void SetIsPauseAutomatic(bool isPauseAutomatic)
    {
        this.isPauseAutomatic = isPauseAutomatic;
    }


    /// <summary>
    /// Displays the moving text canvas in front of the player and loads its text content.
    /// </summary>
    void DisplayMovingCanvasInFront()
    {
        pauseMovingCanvas.GetComponent<Canvas>().enabled = true;
        pauseMovingCanvas.GetComponent<TextDisplayer>().enabled = true;
        if(index == 0)
            pauseMovingCanvas.GetComponent<TextDisplayer>().InitText();
        else
            pauseMovingCanvas.GetComponent<TextDisplayer>().DisplaySpecificSegment(index*2);

        // Debug.Log("Displaying first segment inside AutomaticPauser (DisplayMovingCanvasInFront)");
        StartCoroutine(WaitXSeconds(5f));
    }

    /// <summary>
    /// Waits for a given duration in real time before advancing the text segment.
    /// </summary>
    IEnumerator WaitXSeconds(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        pauseMovingCanvas.GetComponent<TextDisplayer>().NextSegment();
        //Debug.Log("Displaying the second segment inside AutomaticPauser (WaitXSeconds)");
    }

    /// <summary>
    /// Waits for the defined delay, then automatically pauses the simulation,
    /// displays the text canvas, and plays the next audio feedback clip.
    /// Skips execution if the automatic pause was disabled or stopped externally
    /// (e.g., if the user refocused their gaze on the moon before the timer completed).
    /// </summary>
    private IEnumerator PauseRoutine()
    {
        if (!isStoppedExternally && isPauseAutomatic)
        {

            var clips = GetCurrentAudioClips();

            if (clips.Count == 0 || index >= clips.Count)
            {
                isStoppedExternally = true;
            }
            else if (!isPausing)
            {
                yield return new WaitForSecondsRealtime(delayBetweenFeedbacks);

                if (!isPauseAutomatic) yield break;

                DisplayMovingCanvasInFront();
                PlayNextClip();
            }
        }
        pauseRoutine = null;
    }

    /// <summary>
    /// Plays the next audio clip and triggers pause if necessary.
    /// </summary>
    private void PlayNextClip()
    {
        var clips = GetCurrentAudioClips();

        if (clips.Count == 0)
        {
            return;
        }

        if(index >= clips.Count)
        {
            index = 0;
        }

        if (menuPauser != null && !menuPauser.GetInPause())
        {
            menuPauser.SetMenuAudioSource(audioSource);
            menuPauser.StartPause(true);
            audioSource.PlayOneShot(clips[index]);

            isPausing = true;

            index++;
        }
        else
        {
            return;
        }

    }

    /// <summary>
    /// Stops the automatic pause routine, typically called from other scripts to interrupt the sequence.
    /// </summary>
    public void StopCountdownExternally()
    {
        if (pauseRoutine != null)
        {
            isStoppedExternally = true;
            StopCoroutine(pauseRoutine);
            pauseRoutine = null;
        }
    }

    /// <summary>
    /// Resets internal flags when audio playback finishes.
    /// </summary>
    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            isPausing = false;
            isStoppedExternally = false;
        }
    }

    /// <summary>
    /// Returns the appropriate audio list based on the current language.
    /// </summary>
    private List<AudioClip> GetCurrentAudioClips()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? audioClipsFR : audioClips;
    }

}
