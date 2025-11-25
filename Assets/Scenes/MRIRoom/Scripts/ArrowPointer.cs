using UnityEngine;

/// <summary>
/// Rotates an object (e.g., an arrow) to smoothly point toward a target in 3D space.
/// Can optionally aim toward the center of the target's Renderer instead of its pivot.
/// </summary>
public class ArrowPointer : MonoBehaviour
{
    [Tooltip("The target Transform the arrow should point toward.")]
    public Transform target;
    [Tooltip("If true, points toward the center of the target's Renderer instead of its pivot.")]
    public bool useRendererCenter = true;
    [Tooltip("Speed at which the arrow rotates toward the target.")]
    public float rotationSpeed = 5f;

    /// <summary>
    /// Updates the arrow's rotation each frame to face the target smoothly.
    /// </summary>
    void Update()
    {
        if (target == null) return;

        Vector3 targetPoint = target.position;

        // Use the renderer's center if requested
        if (useRendererCenter)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                targetPoint = renderer.bounds.center;
            }
        }

        Vector3 direction = targetPoint - transform.position;

        // Ensure there is a valid direction before rotating
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Adjust orientation if the arrow model points upward (Y+)
            targetRotation *= Quaternion.Euler(90f, 0f, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Optional: draw a line toward the target for debugging
        Debug.DrawLine(transform.position, targetPoint, Color.green);
    }
}
