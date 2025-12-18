using UnityEngine;

/// <summary>
/// Allows manual repositioning and rotation of a reference Transform (typically the XR Origin)
/// using directional input, relative to the camera’s orientation.
/// </summary>
/// <remarks>
/// This component is useful for debugging or manual alignment in XR environments,
/// where it might be needed to fine-tune the user’s origin.
/// <br/><br/>
/// Supports:
/// <list type="bullet">
/// <item>Vertical, forward, and lateral movement relative to the camera orientation.</item>
/// <item>Yaw (horizontal) and pitch (vertical) rotations.</item>
/// <item>UI button integration through public OnMove*/OnRotate* methods.</item>
/// </list>
/// </remarks>
public class ManualRealigner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Transform that will be repositioned and rotated (usually the XR Origin).")]
    [SerializeField] private Transform origin;

    [Tooltip("Reference camera to determine movement orientation.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] private float movementSpeed = 2f;

    [Tooltip("Rotation speed in degrees per second.")]
    [SerializeField] private float rotationSpeed = 45f;

    // --- Internal state flags ---
    private bool moveUp = false;
    private bool moveDown = false;
    private bool moveForward = false;
    private bool moveBackward = false;
    private bool moveLeft = false;
    private bool moveRight = false;
    private bool rotateLeft = false;
    private bool rotateRight = false;
    private bool rotateUp = false;
    private bool rotateDown = false;

    /// <summary>
    /// Updates movement and rotation logic each frame.
    /// </summary>
    private void Update()
    {
        HandleRepositioning();
    }

    /// <summary>
    /// Applies movement and rotation logic based on active directional flags.
    /// </summary>
    private void HandleRepositioning()
    {
        // Recalculate local camera axes
        Vector3 forward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 right = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        // Movement
        if (moveUp) Move(Vector3.up);
        if (moveDown) Move(Vector3.down);
        if (moveForward) Move(forward);
        if (moveBackward) Move(-forward);
        if (moveLeft) Move(-right);
        if (moveRight) Move(right);

        // Rotation
        if (rotateLeft) RotateYaw(-1);
        if (rotateRight) RotateYaw(1);
        if (rotateUp) RotatePitch(-1);
        if (rotateDown) RotatePitch(1);
    }

    /// <summary>
    /// Moves the origin in a given world direction.
    /// </summary>
    /// <param name="direction">Direction vector (normalized).</param>
    private void Move(Vector3 direction)
    {
        Vector3 moveDirection = direction * movementSpeed * Time.unscaledDeltaTime;
        origin.position += moveDirection;
    }

    /// <summary>
    /// Rotates the origin horizontally (yaw) around the global Y axis.
    /// </summary>
    /// <param name="direction">Rotation direction multiplier.</param>
    private void RotateYaw(float direction)
    {
        // Rotation around the camera's local Y-axis
        float rotationAmount = direction * rotationSpeed * Time.unscaledDeltaTime;
        origin.Rotate(Vector3.up, rotationAmount, Space.World);
    }

    /// <summary>
    /// Rotates the origin vertically (pitch) around the camera’s local X axis.
    /// </summary>
    /// <param name="direction">Rotation direction multiplier.</param>
    private void RotatePitch(float direction)
    {
        // Rotation around the camera's local X axis
        float rotationAmount = direction * rotationSpeed * Time.unscaledDeltaTime;

        // Apply rotation
        origin.Rotate(cameraTransform.right, rotationAmount, Space.World);
    }

    // Public UI-linked controls

    /// <summary>Called to move upward.</summary>
    public void OnMoveUp(bool isPressed) => moveUp = isPressed;

    /// <summary>Called to move downward.</summary>
    public void OnMoveDown(bool isPressed) => moveDown = isPressed;

    /// <summary>Called to move forward (relative to camera).</summary>
    public void OnMoveForward(bool isPressed) => moveForward = isPressed;

    /// <summary>Called to move backward (relative to camera).</summary>
    public void OnMoveBackward(bool isPressed) => moveBackward = isPressed;

    /// <summary>Called to move left (relative to camera).</summary>
    public void OnMoveLeft(bool isPressed) => moveLeft = isPressed;

    /// <summary>Called to move right (relative to camera).</summary>
    public void OnMoveRight(bool isPressed) => moveRight = isPressed;

    /// <summary>Called to rotate left (yaw).</summary>
    public void OnRotateLeft(bool isPressed) => rotateLeft = isPressed;

    /// <summary>Called to rotate right (yaw).</summary>
    public void OnRotateRight(bool isPressed) => rotateRight = isPressed;

    /// <summary>Called to rotate upward (pitch).</summary>
    public void OnRotateUp(bool isPressed) => rotateUp = isPressed;

    /// <summary>Called to rotate downward (pitch).</summary>
    public void OnRotateDown(bool isPressed) => rotateDown = isPressed;
}
