using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies a physical pushback when the player's head collides with nearby obstacles,
/// using collision data provided by <see cref="HeadCollisionDetector"/>.
/// </summary>
/// <remarks>
/// This prevents the player's head (and camera) from clipping through walls in VR.
/// The pushback is applied via a <see cref="CharacterController"/> for smooth movement.
/// </remarks>
public class HeadCollisionHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the head collision detector that provides nearby obstacle hits.")]
    [SerializeField]
    private HeadCollisionDetector _detector;

    [Tooltip("CharacterController responsible for applying movement corrections.")]
    [SerializeField]
    private CharacterController _characterController;

    [Header("Pushback Settings")]
    [Tooltip("Strength of the corrective push applied when a collision is detected.")]
    [SerializeField]
    public float pushBackStrength = 1.0f;

    /// <summary>
    /// Calculates the overall pushback direction by averaging all detected surface normals.
    /// </summary>
    private Vector3 CalculatePushBackDirection(List<RaycastHit> colliderHits)
    {
        Vector3 combinedNormal = Vector3.zero;
        foreach (RaycastHit hitPoint in colliderHits)
        {
            combinedNormal +=
                new Vector3(hitPoint.normal.x, 0, hitPoint.normal.z); ;
        }
        return combinedNormal;
    }

    /// <summary>
    /// Continuously checks for nearby obstacles detected by <see cref="HeadCollisionDetector"/> 
    /// and applies a small pushback movement via the CharacterController to prevent clipping.
    /// </summary>
    private void Update()
    {
        if (_detector.DetectedColliderHits.Count <= 0)
        {
            return;
        }
        Vector3 pushBackDirection
            = CalculatePushBackDirection(_detector.DetectedColliderHits);

        Debug.DrawRay(transform.position, pushBackDirection.normalized, Color.magenta);

        _characterController
            .Move(pushBackDirection.normalized * pushBackStrength * Time.deltaTime);
    }
}