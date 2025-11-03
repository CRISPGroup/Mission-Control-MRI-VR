using UnityEngine;

/// <summary>
/// Ensures the attached camera maintains a specified rendering depth,
/// updating automatically both in Play mode and the Unity Editor.
/// </summary>
[ExecuteAlways] // Runs in edit mode and play mode
[RequireComponent(typeof(Camera))]
public class CameraDepthSetter : MonoBehaviour
{
    [Header("Camera Depth Settings")]
    [Tooltip("Rendering order of the camera. Lower = rendered earlier.")]
    [SerializeField] private float cameraDepth = 0f;

    private Camera cam;

    /// <summary>
    /// Called when the script is loaded or a value is changed in the Inspector.
    /// Ensures the camera depth is updated in the Editor.
    /// </summary>
    void OnValidate()
    {
        UpdateCameraDepth();
    }

    /// <summary>
    /// Initializes the camera reference and applies the configured depth.
    /// </summary>
    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraDepth();
    }

    /// <summary>
    /// Updates the camera's rendering depth based on the serialized value.
    /// </summary>
    void UpdateCameraDepth()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null)
            cam.depth = cameraDepth;
    }
}
