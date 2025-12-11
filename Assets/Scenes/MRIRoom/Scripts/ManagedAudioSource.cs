using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages audio playback for bilingual (French/English) content,
/// with UnityEvents triggered at the start and end of playback.
/// Supports playback of clips defined in the Inspector or dynamically provided at runtime.
/// </summary>
/// <remarks>
/// - Automatically switches between French and English clip lists based on <see cref="LanguageManager"/>.<br/>
/// - Exposes both direct and indexed playback methods for easy integration with UI buttons.<br/>
/// - Invokes <see cref="OnStartPlayback"/> when playback begins and <see cref="OnFinishPlayback"/> when playback ends.<br/>
/// - Prevents double invocation when Time.timeScale is paused.
/// </remarks>
class ManagedAudioSource : MonoBehaviour
{
    [Header("Audio Components")]
    [Tooltip("The AudioSource component responsible for playback.")]
    [SerializeField] AudioSource src;

    [Header("Events")]
    [Tooltip("Invoked when audio playback starts.")]
    [SerializeField] UnityEvent OnStartPlayback;

    [Tooltip("Invoked when audio playback finishes.")]
    [SerializeField] UnityEvent OnFinishPlayback;

    [Header("Localized Audio Clips")]
    [Tooltip("Audio clips in French. Indexed playback uses these when language is set to French.")]
    [SerializeField] private AudioClip[] frenchClips;

    [Tooltip("Audio clips in English. Indexed playback uses these when language is set to English.")]
    [SerializeField] private AudioClip[] englishClips;

    bool playing;

    /// <summary>
    /// Ensures the AudioSource reference is assigned at runtime.
    /// </summary>
    void Awake()
    {
        if (src == null) src = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays the specified audio clip at the given volume.
    /// Invokes the start playback event.
    /// </summary>
    /// <param name="clip">The <see cref="AudioClip"/> to play.</param>
    /// <param name="vol">The playback volume (default = 1.0).</param>
    public void Play(AudioClip clip, float vol = 1)
    {
        // Set the clip to the AudioSource and play it
        src.clip = clip;
        src.volume = vol;
        src.Play();

        playing = true;
        OnStartPlayback.Invoke();
    }

    /// <summary>
    /// Plays a specific clip from the Inspector manually.
    /// </summary>
    /// <param name="clip">The <see cref="AudioClip"/> to play.</param>
    public void PlayFromInspector(AudioClip clip)
    {
        if (clip == null) return;

        if (src.mute) src.mute = false;
        Play(clip, 1);
    }

    /// <summary>
    /// Plays an indexed clip from the current language set at a lower volume.
    /// </summary>
    /// <param name="clipIndex">The index of the clip to play.</param>
    public void PlayFromInspectorLowVolume(int clipIndex)
    {
        var clips = GetCurrentAudioClips();

        if (clipIndex >= 0 && clipIndex < clips.Length)
        {
            if (src.mute) src.mute = false;

            float volumeScale = 0.25f;
            Play(clips[clipIndex], volumeScale);
        }
        else
        {
            Debug.LogWarning($"Clip index {clipIndex} out of range for current language.");
        }
    }

    /// <summary>
    /// Plays an indexed clip from the current language set at full volume (100%).
    /// </summary>
    /// <param name="clipIndex">The index of the clip to play.</param>
    public void PlayFromInspector(int clipIndex)
    {
        var clips = GetCurrentAudioClips();

        if (clipIndex >= 0 && clipIndex < clips.Length)
        {
            if (src.mute) src.mute = false;
            Play(clips[clipIndex], 1f);
        }
        else
        {
            Debug.LogWarning($"Clip index {clipIndex} out of range for current language.");
        }
    }

    /// <summary>
    /// Stops the current audio playback and invokes <see cref="OnFinishPlayback"/>.
    /// </summary>
    public void StopAudio()
    {
        if (src != null && src.isPlaying && Time.timeScale != 0f)
        {
            src.Stop();
            playing = false;  // Update the playing flag when stopped
            OnFinishPlayback.Invoke();  // Manually invoke the event when audio stops
        }
    }

    /// <summary>
    /// Monitors playback state each frame and triggers the end event when audio finishes.
    /// </summary>
    void Update()
    {
        if (!playing) return;

        // Check if the AudioSource is still playing
        if (!src.isPlaying && Time.timeScale != 0f)
        {
            playing = false;
            OnFinishPlayback.Invoke();
        }
    }

    /// <summary>
    /// Returns the array of audio clips corresponding to the currently active language.
    /// </summary>
    /// <returns>An array of <see cref="AudioClip"/>s in the correct language.</returns>
    private AudioClip[] GetCurrentAudioClips()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? frenchClips : englishClips;
    }
}
