using UnityEngine;

[ExecuteAlways] // Pour que ça marche aussi dans l’éditeur sans Play mode
[RequireComponent(typeof(Camera))]
public class CameraDepthSetter : MonoBehaviour
{
    [Header("Camera Depth Settings")]
    [Tooltip("Rendering order of the camera. Lower = rendered earlier.")]
    [SerializeField] private float cameraDepth = 0f;

    private Camera cam;

    void OnValidate()
    {
        UpdateCameraDepth();
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraDepth();
    }

    void UpdateCameraDepth()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null)
            cam.depth = cameraDepth;
    }
}
