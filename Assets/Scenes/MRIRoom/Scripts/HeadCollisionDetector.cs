using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Periodically performs ray-based proximity detection around the head (or camera),
/// checking for nearby obstacles or walls to prevent clipping in VR.
/// </summary>
/// <remarks>
/// - Uses 3 rays (forward, right, left) to detect nearby colliders.  
/// - Updates at a configurable interval for performance efficiency.  
/// - Provides visual debug Gizmos to indicate detection state.  
/// </remarks>
public class HeadCollisionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Minimum delay between two detection checks (in seconds).")]
    [SerializeField, Range(0, 0.5f)]
    private float _detectionDelay = 0.05f;

    [Tooltip("Maximum distance for each raycast detection.")]
    [SerializeField]
    private float _detectionDistance = 0.2f;

    [Tooltip("Layer mask used to filter which colliders should be detected.")]
    [SerializeField]
    private LayerMask _detectionLayers;

    /// <summary>
    /// List of currently detected colliders (updated periodically).
    /// </summary>
    public List<RaycastHit> DetectedColliderHits { get; private set; }

    private float _currentTime = 0;

    /// <summary>
    /// Performs a ray-based detection in forward, right, and left directions.
    /// </summary>
    /// <param name="position">Starting position of the rays (typically the head/camera position).</param>
    /// <param name="distance">Maximum raycast distance.</param>
    /// <param name="mask">Layer mask for valid collision targets.</param>
    /// <returns>List of detected RaycastHits within the given parameters.</returns>
    private List<RaycastHit> PreformDetection
    (Vector3 position, float distance, LayerMask mask)
    {
        List<RaycastHit> detectedHits = new();

        List<Vector3> directions
            = new() { transform.forward, transform.right, -transform.right };

        RaycastHit hit;
        foreach (var dir in directions)
        {
            if (Physics.Raycast(position, dir, out hit, distance, mask))
            {
                detectedHits.Add(hit);
            }
        }
        return detectedHits;
    }

    /// <summary>
    /// Initializes the first detection when the scene starts.
    /// </summary>
    private void Start()
    {
        DetectedColliderHits = PreformDetection(transform.position,
           _detectionDistance, _detectionLayers);
    }

    /// <summary>
    /// Periodically updates head collision detection based on the configured delay.
    /// Helps avoid unnecessary raycasts every frame.
    /// </summary>
    void Update()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime > _detectionDelay)
        {
            _currentTime = 0;
            DetectedColliderHits = PreformDetection(transform.position,
                _detectionDistance, _detectionLayers);
        }
    }

    /// <summary>
    /// Draws gizmos to visualize detection rays and proximity sphere in the Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;
        Color c = Color.green;
        c.a = 0.5f;
        if (DetectedColliderHits.Count > 0)
        {
            c = Color.red;
            c.a = 0.5f;
        }

        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, _detectionDistance);

        List<Vector3> directions = new() { transform.forward, transform.right, -transform.right };
        Gizmos.color = Color.magenta;
        foreach (var dir in directions)
        {
            Gizmos.DrawRay(transform.position, dir);
        }
    }
}
