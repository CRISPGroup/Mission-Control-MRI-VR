using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages timed playback of a sequence of audio clips with countdown intervals, looping, 
/// language support (French/English), and event-based progression control.
/// </summary>
/// <remarks>
/// Supports multiple playback modes:
/// - Countdown before playback
/// - Continuous looping with stop condition
/// - Automatic index incrementing based on trip duration (from <see cref="MoonMovement"/>)
/// - Replay or stop at end
/// Also triggers a UnityEvent when a clip finishes playing.
/// </remarks>
public class CountdownAudioPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("List of English audio clips used for playback.")]
    [SerializeField] private List<AudioClip> audioClips;

    [Tooltip("List of French audio clips used for playback.")]
    [SerializeField] private List<AudioClip> audioClipsFR;

    [Header("Audio Source & Events")]
    [Tooltip("AudioSource used for playback.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Event invoked when the current audio clip finishes playing.")]
    [SerializeField] UnityEvent OnFinishPlayback;

    [Header("External References")]
    [Tooltip("Reference to MoonMovement script for trip duration calculation.")]
    [SerializeField] private MoonMovement moonMovementScript;

    // --- Private State ---
    private Coroutine countdownCoroutine;
    private Coroutine audioLoopCoroutine;
    private float countdownTime = 30f;
    private bool isStoppedExternally = false;
    private int currentClipIndex = 0;
    private bool replayAudios = false;
    private bool playing;

    // --- Auto Increment System ---
    private bool enableAutoIncrement = false;
    private float autoIncrementInterval = 0f;
    private float nextAutoIncrementTime = 0f;
    private float tripDuration = 0f;
    private List<int> scheduledIndexes = new List<int>();
    private int scheduledPointer = 0;

    /// <summary>
    /// Ensures that an AudioSource is assigned at startup.
    /// </summary>
    void Start()
    {

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not assigned or found!");
            }
        }
    }

    // ---------------------------------------------------------
    // ------------------- Configuration -----------------------
    // ---------------------------------------------------------


    /// <summary>
    /// Enables or disables looping playback of audio sequences when reaching the end.
    /// </summary>
    public void SetReplayAudios(bool replayAudios)
    {
        this.replayAudios = replayAudios;
    }

    /// <summary>
    /// Manually sets the index of the next audio clip to play.
    /// </summary>
    public void SetCurrentClipIndex(int clipIndex)
    {
        this.currentClipIndex = clipIndex;
    }


    /// <summary>
    /// Sets the countdown timer duration (in seconds) before the next audio clip plays.
    /// </summary>
    public void SetCountDownTimer(float countDownTime)
    {
        this.countdownTime = countDownTime;
    }

    // ---------------------------------------------------------
    // -------------------- External Stop ----------------------
    // ---------------------------------------------------------

    /// <summary>
    /// Stops any active countdown or looping coroutines immediately.
    /// </summary>
    public void StopCountdownExternally()
    {
        if (countdownCoroutine != null)
        {
            isStoppedExternally = true;
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (audioLoopCoroutine != null)
        {
            StopCoroutine(audioLoopCoroutine);
            audioLoopCoroutine = null;
        }

    }

    /// <summary>
    /// Immediately stops the current audio playback (if any).
    /// </summary>
    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            playing = false;
            //OnFinishPlayback.Invoke();
        }
    }
    // ---------------------------------------------------------
    // ------------------- Countdown Play ----------------------
    // ---------------------------------------------------------

    /// <summary>
    /// Starts a countdown before playing a specific clip once.
    /// </summary>
    /// <param name="clipIndex">The index of the clip to play after the countdown.</param>
    public void PlaySpecificClipAfterCountdown(int clipIndex)
    {
        var clips = GetCurrentAudioClips();
        if (clipIndex < 0 || clipIndex >= clips.Count)
        {
            return;
        }

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = StartCoroutine(PlaySpecificClipAfterCountdownRoutine(clipIndex));
    }

    /// <summary>
    /// Coroutine handling the countdown and playback of a specific clip.
    /// </summary>
    private IEnumerator PlaySpecificClipAfterCountdownRoutine(int clipIndex)
    {
        float timer = countdownTime;
        isStoppedExternally = false;

        while (timer > 0f)
        {
            yield return new WaitForSeconds(1f);
            timer--;

            if (isStoppedExternally)
            {
                countdownCoroutine = null;
                yield break;
            }
        }

        if (!isStoppedExternally)
        {
            var clips = GetCurrentAudioClips();
            audioSource.clip = clips[clipIndex];
            audioSource.Play();
            playing = true;
        }
    }

    // ---------------------------------------------------------
    // -------------------- Looping Modes ----------------------
    // ---------------------------------------------------------

    /// <summary>
    /// Starts a countdown, then plays the specified clip in a repeating loop until externally stopped.
    /// </summary>
    public void PlaySpecificClipInLoopAfterCountdown(int clipIndex)
    {
        var clips = GetCurrentAudioClips();
        if (clipIndex < 0 || clipIndex >= clips.Count)
        {
            return;
        }

        isStoppedExternally = false;

        if (countdownCoroutine != null)
        {
            StopCountdownExternally();
        }

        countdownCoroutine = StartCoroutine(PlaySpecificClipInLoopAfterCountdownRoutine(clipIndex));
    }

    /// <summary>
    /// Coroutine that loops playback of a single clip after each countdown period, until externally stopped.
    /// </summary>
    private IEnumerator PlaySpecificClipInLoopAfterCountdownRoutine(int clipIndex)
    {
        while (!isStoppedExternally)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(countdownTime);

            if (!isStoppedExternally)
            {
                var clips = GetCurrentAudioClips();
                audioSource.clip = clips[clipIndex];
                audioSource.Play();
                playing = true;
            }
        }
        countdownCoroutine = null;
    }

    // ---------------------------------------------------------
    // ---------------- Sequential Playback -------------------
    // ---------------------------------------------------------

    /// <summary>
    /// Starts a countdown and plays the next clip in sequence once.
    /// </summary>
    public void PlayNextClipAfterCountdown(bool waitForPreviousClipToFinish = true)
    {
        isStoppedExternally = false;
        if (countdownCoroutine != null)
        {
            StopCountdownExternally();
        }

        countdownCoroutine = StartCoroutine(CountdownRoutine(waitForPreviousClipToFinish));
    }

    /// <summary>
    /// Coroutine that waits for an optional previous clip, then plays the next one after the countdown delay.
    /// </summary>
    private IEnumerator CountdownRoutine(bool waitForPreviousClipToFinish)
    {
        if (!isStoppedExternally)
        {
            if(waitForPreviousClipToFinish)
            {
                yield return new WaitWhile(() => audioSource.isPlaying);
            }

            yield return new WaitForSeconds(countdownTime);

            var clips = GetCurrentAudioClips();

            if (currentClipIndex >= clips.Count && !replayAudios)
            {
                yield break;
            }

            if (currentClipIndex >= clips.Count && replayAudios)
            {
                currentClipIndex = 0;
            }

            if (!isStoppedExternally)
            {
                //Debug.Log("is playing: " + audioClips[currentClipIndex] + "at index: " + currentClipIndex);
                audioSource.clip = clips[currentClipIndex];
                audioSource.Play();
                playing = true;
                currentClipIndex = currentClipIndex + 1;
            }
        }
        countdownCoroutine = null;
    }

    /// <summary>
    /// Continuously plays clips in sequence after each countdown, until externally stopped.
    /// </summary>
    public void PlayNextClipAfterCountdownUntilStop(bool waitForPreviousClipToFinish = true)
    {
        isStoppedExternally = false;
        if (countdownCoroutine != null)
        {
            StopCountdownExternally();
        }

        countdownCoroutine = StartCoroutine(CountdownRoutineUntilStop(waitForPreviousClipToFinish));
    }

    /// <summary>
    /// Coroutine that loops through audio clips sequentially until manually stopped.
    /// </summary>
    private IEnumerator CountdownRoutineUntilStop(bool waitForPreviousClipToFinish)
    {
        while (!isStoppedExternally)
        {
            if (waitForPreviousClipToFinish){
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            
            yield return new WaitForSeconds(countdownTime);

            var clips = GetCurrentAudioClips();

            if (currentClipIndex >= clips.Count && !replayAudios)
            {
                yield break;
            }

            if (currentClipIndex >= clips.Count && replayAudios)
            {
                currentClipIndex = 0;
            }

            if (!isStoppedExternally)
            {
                //Debug.Log("is playing: " + audioClips[currentClipIndex] + " at index: " + currentClipIndex);
                audioSource.clip = clips[currentClipIndex];
                audioSource.Play();
                playing = true;

                if (!enableAutoIncrement) // Only increment if auto-increment is disabled
                {
                    currentClipIndex++;
                }
            }
        }

        countdownCoroutine = null;
    }

    // ---------------------------------------------------------
    // ----------------- Auto Increment System -----------------
    // ---------------------------------------------------------

    /// <summary>
    /// Begins automatic clip index scheduling based on trip duration from <see cref="MoonMovement"/>.
    /// </summary>
    public void StartIndexIncrementCountdown()
    {
        if (moonMovementScript == null)
        {
            Debug.LogError("MoonMovement script not assigned or incorrect type.");
            return;
        }

        tripDuration = moonMovementScript.GetDuration();
        var clips = GetCurrentAudioClips();

        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("Invalid audio clip list.");
            return;
        }

        // --- Select clip indexes depending on trip duration
        scheduledIndexes.Clear();

        if (Mathf.Approximately(tripDuration, 120f))
        {
            scheduledIndexes.AddRange(new int[] { 2, 4, 6 });
        }
        else if (Mathf.Approximately(tripDuration, 180f))
        {
            scheduledIndexes.AddRange(new int[] { 0, 2, 4, 6, 8 });
        }
        else if (Mathf.Approximately(tripDuration, 240f))
        {
            scheduledIndexes.AddRange(new int[] { 0, 2, 3, 4, 5, 6, 8 });
        }
        else if (tripDuration >= 300f)
        {
            // 5mins or more: all the clips (once)
            for (int i = 0; i < clips.Count; i++)
                scheduledIndexes.Add(i);
        }
        else
        {
            Debug.LogWarning($"[AutoIncrement] Unexpected duration: {tripDuration}");
        }

        // --- Increment settings ---
        autoIncrementInterval = 30f; // Every 30 seconds
        nextAutoIncrementTime = Time.time + autoIncrementInterval;
        currentClipIndex = 0;
        enableAutoIncrement = true;

        //Debug.Log($"[AutoIncrement] Trip={tripDuration}s | Scheduled={scheduledIndexes.Count} clips | Every {autoIncrementInterval}s");
    }

    /// <summary>
    /// Stops automatic clip index scheduling.
    /// </summary>
    public void StopIndexIncrementCountdown()
    {
        //Debug.Log("StopIndexIncrementCountdown.");
        enableAutoIncrement = false;
        //currentClipIndex = 0;
    }

    // ---------------------------------------------------------
    // -------------------- Update Loop ------------------------
    // ---------------------------------------------------------

    /// <summary>
    /// Monitors audio playback completion, invokes OnFinishPlayback, and handles scheduled index increments.
    /// </summary>
    void Update()
    {
        // Detect clip completion
        if (playing && !audioSource.isPlaying){
            playing = false;
            isStoppedExternally = false;
            OnFinishPlayback.Invoke();
        }

        // Handle automatic index incrementing
        if (enableAutoIncrement && scheduledPointer < scheduledIndexes.Count)
        {
            // Triggers the index change 1s before the exact moment
            while (Time.time >= nextAutoIncrementTime - 1f && scheduledPointer < scheduledIndexes.Count)
            {
                int clipIndex = scheduledIndexes[scheduledPointer];
                this.currentClipIndex = clipIndex;

                //Debug.Log($"[AutoIncrement]: Set currentClipIndex = {clipIndex} (scheduledPointer {scheduledPointer}) at {Time.time - Time.timeSinceLevelLoad:F1}s");

                // We advance the pointer for the next iteration
                scheduledPointer++;
                nextAutoIncrementTime += autoIncrementInterval;
            }
        }

        if (enableAutoIncrement && scheduledPointer >= scheduledIndexes.Count)
        {
            //Debug.Log("[AutoIncrement] All scheduled indexes reached.");
            StopIndexIncrementCountdown();
        }
    }

    /// <summary>
    /// Returns the currently active list of audio clips based on the selected language.
    /// </summary>
    private List<AudioClip> GetCurrentAudioClips()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? audioClipsFR : audioClips;
    }
}
