using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Handles the animated movement, scaling, and rotation of a “moon” object
/// between two positions over a specified duration.
/// </summary>
/// <remarks>
/// This component interpolates the moon’s position, scale, and rotation speed
/// from a defined start state to an end state.  
/// It also dynamically scales a target collider so its visual size remains
/// consistent despite the moon’s scaling.
/// <br/><br/>
/// Typical usage:
/// <list type="bullet">
/// <item>Assign <see cref="moonStartPosition"/> and <see cref="moonEndPosition"/> in the Inspector.</item>
/// <item>Call <see cref="StartMovement"/> to begin the motion sequence.</item>
/// <item>Subscribe to <see cref="OnFinishPlayback"/> for when the movement ends.</item>
/// </list>
/// </remarks>
public class MoonMovement : MonoBehaviour
{
    private Vector3 startPosition;

    [Header("Transforms")]
    [Tooltip("The starting position transform of the moon.")]
    [SerializeField] private Transform moonStartPosition;

    [Tooltip("The ending position transform of the moon.")]
    [SerializeField] private Transform moonEndPosition;
    private Vector3 endPosition;

    [Header("Scaling")]
    [Tooltip("The initial scale of the moon.")]
    [SerializeField] private Vector3 startScale = new Vector3(1f, 1f, 1f);

    [Tooltip("The final scale of the moon at the end of the movement.")]
    [SerializeField] private Vector3 endScale = new Vector3(7f, 7f, 7f);

    [Header("Timing and Rotation")]
    [Tooltip("Total duration of the movement (in seconds).")]
    [SerializeField] private float duration = 103f;

    [Tooltip("Initial rotational speed (degrees per second).")]
    [SerializeField] private float startRotationSpeed = 6f;

    [Header("Target Collider Scaling")]
    [Tooltip("Reference to the target collider GameObject to scale proportionally.")]
    [SerializeField] private GameObject targetCollider;

    [Tooltip("Base size multiplier for the collider’s visual scale.")]
    [SerializeField] private float targetBaseVisualSize = 0.01f;

    [Header("Events")]
    [Tooltip("Event invoked when the moon finishes its movement.")]
    [SerializeField] UnityEvent OnFinishPlayback;

    private float startTime;

    private bool isMoving = false;

    // -------------------------------
    // Public Methods
    // -------------------------------

    /// <summary>
    /// Resets the moon’s position and scale to the defined starting state.
    /// </summary>
    public void ResetPositionAndScale ()
    {
        startPosition = moonStartPosition.position;
        transform.position = startPosition;
        transform.localScale = startScale;
    }

    /// <summary>
    /// Starts the moon movement animation sequence.
    /// </summary>
    public void StartMovement()
    {
        startPosition = moonStartPosition.position;
        endPosition = moonEndPosition.position;

        transform.position = startPosition;
        transform.localScale = startScale;
        //targetCollider.transform.localScale = startTargetColliderScale;

        startTime = Time.time;
        isMoving = true;
    }

    /// <summary>
    /// Manually enables or disables the movement animation.
    /// </summary>
    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    /// <summary>
    /// Sets the duration of the movement.
    /// </summary>
    /// <param name="newDuration">The new duration, in seconds.</param>
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    /// <summary>
    /// Returns the current movement duration.
    /// </summary>
    public float GetDuration()
    {
        return duration;
    }

    // -------------------------------
    // Internal Logic
    // -------------------------------

    /// <summary>
    /// Updates the moon’s position, scale, rotation, and target collider size every frame
    /// while the movement animation is active.
    /// </summary>
    void Update()
    {
        if (isMoving)
        {
            // 1. Compute elapsed normalized time
            float timeElapsed = Time.time - startTime;
            float t = Mathf.Clamp01(timeElapsed / duration);

            // 2. Interpolate position
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // 3. Interpolate scale
            Vector3 scale = Vector3.Lerp(startScale, endScale, t);
            transform.localScale = scale;

            // 4. Inverse-scaling of target collider to maintain visual size
            float inverseScaleFactor = 1f / transform.localScale.x;
            const float fixedY = 0.0015f;

            targetCollider.transform.localScale = new Vector3(
                inverseScaleFactor * targetBaseVisualSize,
                fixedY,
                inverseScaleFactor * targetBaseVisualSize
            );

            // 5. Gradually reduce rotation speed
            float rotationSpeed = Mathf.Lerp(startRotationSpeed, 0f, t);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            if (timeElapsed >= duration)
            {
                OnFinish();
            }
        }
    }

    /// <summary>
    /// Stops the moon’s movement and triggers the completion event.
    /// </summary>
    public void OnFinish()
    {
        isMoving = false;
        OnFinishPlayback.Invoke();
    }

}
