using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when the player (or camera) is looking in a specified direction within a given angle threshold.
/// Invokes corresponding UnityEvents when the look direction is confirmed or unconfirmed.
/// Useful for gaze-based interactions, calibration, or direction-based triggers in VR.
/// </summary>
public class ConfirmLookDirection : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Reference to the player's camera (VR or standard).")]
    [SerializeField] private Transform vrCamera;

    [Header("Target Direction Settings")]
    [Tooltip("Target world direction to check against the camera's forward vector.")]
    [SerializeField] private Vector3 targetDirection = Vector3.up;

    [Tooltip("Maximum accepted angle (in degrees) between the camera's forward vector and the target direction.")]
    [SerializeField] private float angleThreshold = 15f;

    [Header("Events")]
    [Tooltip("Event invoked when the user looks within the allowed angle of the target direction.")]
    [SerializeField] private UnityEvent onLookConfirmed;

    [Tooltip("Event invoked when the user looks away or outside the angle threshold.")]
    [SerializeField] private UnityEvent onLookUnconfirmed;

    /// <summary>
    /// Validates the camera reference at startup.
    /// </summary>
    void Start()
    {
        if (vrCamera == null)
        {
            Debug.LogError("VR camera is not assigned!");
        }
    }

    /// <summary>
    /// Checks each frame whether the camera is facing the target direction within the specified angle threshold.
    /// Invokes the corresponding UnityEvent based on the result.
    /// </summary>
    void Update()
    {
        if (vrCamera == null) return;

        float angle = Vector3.Angle(vrCamera.forward, targetDirection);

        if (angle <= angleThreshold)
        {
            onLookConfirmed?.Invoke();
        }
        else
        {
            onLookUnconfirmed?.Invoke();
        }
    }

    /// <summary>
    /// Dynamically updates the target direction and optional angle threshold at runtime.
    /// </summary>
    /// <param name="newDirection">New target world-space direction to track.</param>
    /// <param name="newThreshold">Optional new acceptance angle (defaults to 15°).</param>
    public void SetTargetDirection(Vector3 newDirection, float newThreshold = 15f)
    {
        targetDirection = newDirection;
        angleThreshold = newThreshold;
    }
}
