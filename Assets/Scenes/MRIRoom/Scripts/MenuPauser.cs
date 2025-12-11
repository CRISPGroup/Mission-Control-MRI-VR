using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Windows;

/// <summary>
/// Manages game pausing, including time scaling, audio and video state,
/// and displaying an in-VR pause menu in front of the player.
/// </summary>
/// <remarks>
/// This component supports both manual and automatic pauses (e.g., system focus loss).
/// It can:
/// <list type="bullet">
/// <item>Pause and resume gameplay (via <see cref="Time.timeScale"/>).</item>
/// <item>Pause and resume all active <see cref="AudioSource"/> and <see cref="VideoPlayer"/> instances.</item>
/// <item>Display a menu canvas aligned with the user’s current gaze direction.</item>
/// <item>Trigger UnityEvents when entering or exiting pause.</item>
/// </list>
/// </remarks>
public class MenuPauser : MonoBehaviour
{
    [Header("Menu References")]
    [Tooltip("Main canvas of the pause menu.")]
    [SerializeField] private GameObject menuCanvas;

    [Tooltip("Camera object used to render the pause menu (usually a world-space camera).")]
    [SerializeField] private GameObject menuCamera;

    [Tooltip("Dedicated AudioSource used to play pause-related sounds.")]
    [SerializeField] private AudioSource menuAudioSource;

    [Tooltip("Reference to CanvasActivator (optional, used for enabling/disabling UI canvases).")]
    [SerializeField] private CanvasActivator canvasActivator;

    [Tooltip("Button used to return to the main menu after unpausing.")]
    [SerializeField] private Button backToMainButton;

    [Header("Audio Clips")]
    [Tooltip("Audio clip played when pausing manually during the Moon Trip sequence (English).")]
    [SerializeField] private AudioClip moonTripManualPauseAudio;

    [Tooltip("Audio clip played when pausing manually during the Moon Trip sequence (French).")]
    [SerializeField] private AudioClip moonTripManualPauseAudioFr;

    [Header("Pause Events")]
    [Tooltip("Invoked when the pause process begins.")]
    [SerializeField] UnityEvent OnEnterPause;

    [Tooltip("Invoked when resuming from pause.")]
    [SerializeField] UnityEvent OnFinishPause;

    private bool inPause = false;
    private CanvasGroup canvasGroup;
    private bool isMoonTrip = false; //If pause happens during moon trip

    private bool canShowMenu = true;

    private bool isPauseSystem = false;
    private List<VideoPlayer> pausedVideos = new List<VideoPlayer>();

    /// <summary>
    /// Initializes the menu’s <see cref="CanvasGroup"/> component used to
    /// control interactivity and raycast blocking during pause.
    /// </summary>
    void Start()
    {
        canvasGroup = menuCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is missing on the menuCanvas GameObject.");
        }
    }

    // ---------------------------
    // Configuration Methods
    // ---------------------------

    /// <summary>Marks this pause as triggered by a system (e.g., when pressing the meta pause button or when pressing the sleep button on the headset).</summary>
    public void SetIsPauseSystem(bool isPauseSystem)
    {
        this.isPauseSystem = isPauseSystem;
    }

    /// <summary>Controls whether the menu can currently be displayed.</summary>
    public void SetMenuReady(bool ready)
    {
        canShowMenu = ready;
    }

    /// <summary>Sets the AudioSource used for menu sound playback.</summary>
    public void SetMenuAudioSource(AudioSource audioSource)
    {
        this.menuAudioSource = audioSource;
    }

    /// <summary>Sets whether the pause occurs during a Moon Trip session.</summary>
    public void SetIsMoonTrip(bool isMoonTrip)
    {
        this.isMoonTrip = isMoonTrip;
    }

    // ---------------------------
    // Pause Management
    // ---------------------------

    /// <summary>
    /// Toggles between pause and unpause states.
    /// </summary>
    public void PerformPauseAction()
    {
        if (inPause)
        {
            ExitPause();
        }
        else
        {
            StartPause();
        }
    }
    /// <summary>
    /// Initiates a pause. Can be called manually or automatically (focus loss, etc.).
    /// </summary>
    /// <param name="isAutomaticPause">True if triggered automatically (e.g., by system focus loss).</param>
    public void StartPause(bool isAutomaticPause = false)
    {
        if (!canShowMenu)
        {
            return;
        }

        OnEnterPause.Invoke();

        PauseGame();
        if (isMoonTrip && !isAutomaticPause && !isPauseSystem)
        {
            menuAudioSource.PlayOneShot(GetCurrentMoonClip());
        }

        if (!isAutomaticPause)
        {
            DisplayMenuInFront();
        }

        StartCoroutine(ActivateMenuCameraDelayed());
    }

    /// <summary>
    /// Coroutine used to delay menu camera activation by one frame.
    /// </summary>
    private IEnumerator ActivateMenuCameraDelayed()
    {
        // Attendre un frame pour s'assurer que tout est bien initialisé
        yield return null;
        menuCamera.SetActive(true);
        inPause = true;
    }

    /// <summary>
    /// Exits pause mode, resumes game state, and invokes the exit event.
    /// </summary>
    public void ExitPause()
    {
        DesactivateMenuInFront();
        UnpauseGame();
        menuCamera.SetActive(false);
        OnFinishPause.Invoke();
        inPause = false;
        backToMainButton.onClick.Invoke();
    }

    /// <summary>
    /// Hides the pause menu canvas.
    /// </summary>
    public void DesactivateMenuInFront()
    {
        menuCanvas.GetComponent<Canvas>().enabled = false;
        //menuCanvas.SetActive(false);
    }

    /// <summary>
    /// Positions the pause menu in front of the player’s current view.
    /// </summary>
    public void DisplayMenuInFront()
    {
        Vector3 vHeadPos = Camera.main.transform.position;
        Vector3 vGazeDir = Camera.main.transform.forward;
        menuCanvas.transform.position = (vHeadPos + vGazeDir * 3.0f) + new Vector3(0.0f, -.40f, 0.0f);
        Vector3 vRot = Camera.main.transform.eulerAngles; vRot.z = 0;
        menuCanvas.transform.eulerAngles = vRot;

        //menuCanvas.SetActive(true);
        menuCanvas.GetComponent<Canvas>().enabled = true;
    }

    // ---------------------------
    // Time & Media Control
    // ---------------------------

    /// <summary>
    /// Pauses gameplay time, all non-menu audio sources, and any playing videos.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            if (audioSource != menuAudioSource && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        VideoPlayer[] videoPlayers = FindObjectsOfType<VideoPlayer>();
        foreach (var videoPlayer in videoPlayers)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                pausedVideos.Add(videoPlayer);
            }
        }
    }

    /// <summary>
    /// Resumes gameplay time, audio, and previously paused videos.
    /// </summary>
    public void UnpauseGame()
    {
        Time.timeScale = 1.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            audioSource.UnPause();
        }

        foreach (var video in pausedVideos)
        {
            if (video != null)
                video.Play();
        }
        pausedVideos.Clear();
    }

    /// <summary>
    /// Returns whether the game is currently paused.
    /// </summary>
    public bool GetInPause()
    {
        return inPause;
    }

    /// <summary>
    /// Retrieves the localized Moon Trip pause clip depending on current language.
    /// </summary>
    private AudioClip GetCurrentMoonClip()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? moonTripManualPauseAudioFr : moonTripManualPauseAudio;
    }
}
