using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

/// <summary>
/// Manages the playback of a <see cref="VideoPlayer"/> component with UnityEvent hooks
/// for start and end of playback. Supports direct clip assignment and Inspector-based control.
/// </summary>
/// <remarks>
/// - Automatically subscribes to <see cref="VideoPlayer.loopPointReached"/> to detect playback completion.<br/>
/// - Invokes <see cref="OnStartPlayback"/> when playback starts and <see cref="OnFinishPlayback"/> when the video ends or is stopped.<br/>
/// - Can be safely reused across multiple video clips.
/// </remarks>
public class ManagedVideoPlayer : MonoBehaviour
{
    [Header("Video Components")]
    [Tooltip("The VideoPlayer component that will handle video playback.")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Playback Events")]
    [Tooltip("Invoked when video playback starts.")]
    [SerializeField] private UnityEvent OnStartPlayback;

    [Tooltip("Invoked when video playback finishes or stops.")]
    [SerializeField] private UnityEvent OnFinishPlayback;

    private bool playing;

    /// <summary>
    /// Initializes the component and ensures required references are set.
    /// </summary>
    void Awake()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        // Subscribe to the loopPointReached event which is triggered when the video finishes playing
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    /// <summary>
    /// Plays the specified video clip.
    /// </summary>
    /// <param name="clip">The <see cref="VideoClip"/> to play.</param>
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

    /// <summary>
    /// Allows triggering playback directly from the Unity Inspector.
    /// </summary>
    /// <param name="clip">The <see cref="VideoClip"/> to play.</param>
    public void PlayFromInspector(VideoClip clip)
    {
        Play(clip);
    }

    /// <summary>
    /// Stops the current video playback and invokes the finish event manually.
    /// </summary>
    public void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            playing = false;
            OnFinishPlayback.Invoke();
        }
    }

    /// <summary>
    /// Runtime checks or updates during playback.
    /// </summary>
    void Update()
    {
        if (!playing) return;

        // Optionally, handle any updates needed during playback here
    }

    /// <summary>
    /// Called automatically when the video reaches its end.
    /// </summary>
    /// <param name="source">The VideoPlayer that triggered the event.</param>
    private void OnVideoFinished(VideoPlayer source)
    {
        playing = false;
        OnFinishPlayback.Invoke();
    }

    /// <summary>
    /// Unsubscribes from Unity events.
    /// </summary>
    void OnDestroy()
    {
        // It's a good practice to unsubscribe from events when the object is destroyed
        videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
