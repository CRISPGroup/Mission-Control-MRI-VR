using UnityEngine;
using System.Collections;

/// <summary>
/// Moves the XR Rig in sync with this object’s transform, allowing indirect dragging of the player rig.
/// Typically used when an interactable object (e.g., a bed or platform) is moved, 
/// and the XR Rig should follow that movement smoothly in world space.
/// </summary>
public class DragXRRig : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the XR Rig Transform to move in sync with this object.")]
    [SerializeField] private Transform xRRigTransform;

    [Tooltip("Movement speed multiplier applied to the XR Rig while dragging.")]
    [SerializeField] private float dragSpeed = 1.0f;

    [Tooltip("Initial position reference (e.g., the bed starting point).")]
    [SerializeField] private Transform bedInitialPosition;

    private Vector3 previousPosition;
    private bool isDragging = false;

    /// <summary>
    /// Initializes the object's position to match the initial bed position and records it for tracking.
    /// </summary>
    void Start()
    {
        transform.position = bedInitialPosition.position;
        previousPosition = transform.position;
    }

    /// <summary>
    /// Checks if the object has moved since the last frame and starts dragging if movement is detected.
    /// </summary>
    void Update()
    {
        if (!isDragging && transform.position != previousPosition)
        {
            PerformDragging();
        }
    }

    /// <summary>
    /// Begins the dragging process and starts the coroutine that updates XR Rig movement.
    /// </summary>
    public void PerformDragging()
    {
        //Debug.Log("Perform Dragging..");
        previousPosition = transform.position;
        isDragging = true;
        StartCoroutine(DoDragging());
    }

    /// <summary>
    /// Coroutine that continuously updates the XR Rig position to follow this object’s movement.
    /// Runs every frame until dragging is stopped.
    /// </summary>
    IEnumerator DoDragging()
    {
        while (isDragging)
        {
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - previousPosition;

            // Apply movement to the XR Rig
            xRRigTransform.position += movement;

            previousPosition = currentPosition;

            yield return null;
        }

        isDragging = false;
    }
}
