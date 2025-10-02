using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class ManagedVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private UnityEvent OnStartPlayback;
    [SerializeField] private UnityEvent OnFinishPlayback;

    private bool playing;

    void Awake()
    {
        // Ensure the videoPlayer component is attached
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        // Subscribe to the loopPointReached event which is triggered when the video finishes playing
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    public void Play(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("No video clip specified");
            return;
        }

        // Set the video clip and prepare the player
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        // Wait until the video is prepared before starting playback
        videoPlayer.prepareCompleted += (source) =>
        {
            videoPlayer.Play();
            playing = true;
            OnStartPlayback.Invoke();
        };
    }

    public void PlayFromInspector(VideoClip clip)
    {
        Play(clip);
    }

    public void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            playing = false;
            OnFinishPlayback.Invoke(); // Invoke the finish event when video stops
        }
    }

    void Update()
    {
        if (!playing) return;

        // Optionally, handle any updates needed during playback here
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        playing = false;
        OnFinishPlayback.Invoke();
    }

    void OnDestroy()
    {
        // It's a good practice to unsubscribe from events when the object is destroyed
        videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
