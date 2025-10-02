using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features.Meta;

public class GraphicsImprover : MonoBehaviour
{
    public enum HeadsetType { Unknown, Quest1, Quest2, Quest3, QuestPro }
    private XRDisplaySubsystem display;


    void Awake()
    {
        #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                Debug.unityLogger.logEnabled = false;
        #endif

        var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = GetTargetRenderScale();
        InitXRDisplay();
        SetRefreshRate();
        SetFoveatedRenderingLevel(1);
    }
    private void InitXRDisplay()
    {
        var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
        if (loader != null)
        {
            display = loader.GetLoadedSubsystem<XRDisplaySubsystem>();
            if (display != null)
            {
                Debug.Log("[GraphicsImprover] XRDisplaySubsystem actif.");
            }
            else
            {
                Debug.LogWarning("[GraphicsImprover] Aucun XRDisplaySubsystem trouvé.");
            }
        }
    }
    public void SetRefreshRate()
    {
        if (display == null) return;

        if (DetectHeadset() == HeadsetType.Quest3)
            display.TryRequestDisplayRefreshRate(90f);
        else
            display.TryRequestDisplayRefreshRate(72f);
    }

    public void SetFoveatedRenderingLevel(int level)
    {
        if (display == null) return;

        display.foveatedRenderingLevel = level;
        display.foveatedRenderingFlags =
                    XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
    }

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

    private float GetTargetRenderScale()
    {
        switch (DetectHeadset())
        {
            case HeadsetType.Quest3:
                return 1.2f;
            default:
                return 1.0f;
        }
    }

    /*void Start()
    {
        //UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.5f;
    }*/

    static void ResetRenderScale()
    {
        var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        urp.renderScale = 1f;
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("Language");
        PlayerPrefs.Save();
        ResetRenderScale();
    }

    #if UNITY_EDITOR
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
