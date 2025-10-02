using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

class ManagedAudioSource : MonoBehaviour
{
    [SerializeField] AudioSource src;
    [SerializeField] UnityEvent OnStartPlayback;
    [SerializeField] UnityEvent OnFinishPlayback;

    [SerializeField] private AudioClip[] frenchClips;
    [SerializeField] private AudioClip[] englishClips;

    bool playing;

    void Awake()
    {
        if (src == null) src = GetComponent<AudioSource>();

    }

    private void Start()
    {

    }

    public void Play(AudioClip clip, float vol = 1)
    {
        // Set the clip to the AudioSource and play it
        src.clip = clip;
        src.volume = vol;
        src.Play();

        playing = true;
        OnStartPlayback.Invoke();
    }

    public void PlayFromInspector(AudioClip clip)
    {
        if (clip == null) return;

        if (src.mute) src.mute = false;
        Play(clip, 1);
    }

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

    public void StopAudio()
    {
        if (src != null && src.isPlaying && Time.timeScale != 0f)
        {
            src.Stop();
            playing = false;  // Update the playing flag when stopped
            OnFinishPlayback.Invoke();  // Manually invoke the event when audio stops
        }
    }

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

    private AudioClip[] GetCurrentAudioClips()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? frenchClips : englishClips;
    }
}
