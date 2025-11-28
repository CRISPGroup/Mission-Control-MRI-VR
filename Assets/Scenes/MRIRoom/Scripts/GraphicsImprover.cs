using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features.Meta;

/// <summary>
/// Automatically configures graphics and XR settings for optimal performance on Meta Quest headsets,
/// and exposes UnityEvents for pause and focus transitions.
/// </summary>
/// <remarks>
/// <para>This component dynamically adjusts URP render scale, refresh rate, and foveated rendering
/// depending on the detected headset model.</para>
/// <para>It also triggers UnityEvents when the application pauses or loses focus,
/// allowing developers to hide controllers, pause simulations, or mute audio.</para>
/// </remarks>
public class GraphicsImprover : MonoBehaviour
{
    /// <summary>
    /// Supported headset types used for automatic optimization.
    /// </summary>
    public enum HeadsetType { Unknown, Quest1, Quest2, Quest3, QuestPro }

    private XRDisplaySubsystem display;

    [Header("On Pause Events")]
    [Tooltip("Invoked when the application is paused (e.g., user removes headset).")]
    public UnityEvent onPaused;

    [Tooltip("Invoked when the application resumes after being paused.")]
    public UnityEvent onResumed;

    [Header("On Focus Events")]
    [Tooltip("Invoked when the application loses focus (e.g., Meta menu or overlay opened).")]
    [SerializeField] private UnityEvent onFocusLost;

    [Tooltip("Invoked when the application regains focus.")]
    [SerializeField] private UnityEvent onFocusGained;

    private bool isFocused = true;

    /// <summary>
    /// Initializes application-level graphics and XR settings when the scene loads.
    /// This method runs before Start() and configures the render pipeline for optimal performance.
    /// </summary>
    void Awake()
    {
        #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                Debug.unityLogger.logEnabled = false;
        #endif

        var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = GetTargetRenderScale();
        InitXRDisplay();
        SetRefreshRate();
        SetFoveatedRenderingLevel(2);
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    /// <summary>
    /// Initializes the XRDisplaySubsystem reference if XR is active.
    /// </summary>
    private void InitXRDisplay()
    {
        var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
        if (loader != null)
        {
            display = loader.GetLoadedSubsystem<XRDisplaySubsystem>();
            if (display == null)
            {
                Debug.LogWarning("[GraphicsImprover] No XRDisplaySubsystem found.");
            }
        }
    }

    /// <summary>
    /// Adjusts the device refresh rate and texture resolution scale based on the detected headset.
    /// </summary>
    public void SetRefreshRate()
    {
        XRSettings.eyeTextureResolutionScale = 0.8f;

        if (display == null) return;

        if (DetectHeadset() == HeadsetType.Quest3)
            display.TryRequestDisplayRefreshRate(90f);
        else
            display.TryRequestDisplayRefreshRate(72f);
    }

    /// <summary>
    /// Configures the headset’s foveated rendering level and enables gaze-based foveation.
    /// </summary>
    public void SetFoveatedRenderingLevel(int level)
    {
        if (display == null) return;

        display.foveatedRenderingLevel = level;
        display.foveatedRenderingFlags =
                    XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
    }

    /// <summary>
    /// Detects the current headset model based on Android device identifiers.
    /// </summary>
    public static HeadsetType DetectHeadset()
    {
        if (Application.platform != RuntimePlatform.Android)
            return HeadsetType.Unknown;

        if (SystemInfo.deviceModel != "Oculus Quest")
            return HeadsetType.Unknown;

        var build = new AndroidJavaClass("android.os.Build");
        string device = build.GetStatic<string>("DEVICE");

        return device switch
        {
            "miramar" => HeadsetType.Quest1,
            "hollywood" => HeadsetType.Quest2,
            "eureka" => HeadsetType.Quest3,
            "cambria" => HeadsetType.QuestPro,
            _ => HeadsetType.Unknown
        };
    }

    /// <summary>
    /// Returns the target URP render scale depending on the detected headset.
    /// </summary>
    private float GetTargetRenderScale()
    {
        XRSettings.eyeTextureResolutionScale = 0.9f;
        switch (DetectHeadset())
        {
            case HeadsetType.Quest3:
                return 1.2f;
            default:
                return 1.1f;
        }
    }

    /// <summary>
    /// Resets URP render scale to 1.0 (default) when exiting play mode or application quit.
    /// </summary>
    static void ResetRenderScale()
    {
        var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = 1f;
    }

    /// <summary>
    /// Called automatically when the application is paused or resumed 
    /// (e.g., when the headset is removed or the app goes into the background).
    /// </summary>
    /// <param name="pauseStatus">True if the app is paused, false if resumed.</param>
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            onPaused?.Invoke();
        }
        else
        {
            onResumed?.Invoke();
        }
    }

    /// <summary>
    /// Called when the application loses or regains focus 
    /// (e.g., Meta Home button pressed, system overlay, or casting interruption).
    /// </summary>
    /// <param name="hasFocus">True if the app regains focus, false if another app takes focus.</param>
    void OnApplicationFocus(bool hasFocus)
    {
        isFocused = hasFocus;

        if (!hasFocus)
        {
            onFocusLost?.Invoke();
        }
        else
        {
            onFocusGained?.Invoke();
        }
    }

    /// <summary>
    /// Called when the application is about to quit.
    /// Resets graphics settings and clears temporary PlayerPrefs values.
    /// </summary>
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("Language");
        PlayerPrefs.Save();
        ResetRenderScale();
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Ensures render scale resets properly after exiting Play Mode in the Editor.
    /// </summary>
    [UnityEditor.InitializeOnLoadMethod]
        static void ResetRenderScaleInEditor()
        {
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    ResetRenderScale();
                }
            };
        }
    #endif
}
