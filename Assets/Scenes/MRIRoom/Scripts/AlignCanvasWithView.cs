using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Keeps a world-space canvas facing the VR camera at a fixed distance,
/// with optional horizontal-only rotation and smooth interpolation.
/// </summary>
public class AlignCanvasWithView : MonoBehaviour
{
    [SerializeField] private Transform vrCamera; // Reference to the VR camera
    [SerializeField] private Transform canvasTransform; // Canvas to align
    [SerializeField] private float smoothSpeed = 5f; // Lerp speed
    [SerializeField] private float distanceFromCamera = 2f; // Distance from camera
    [SerializeField] private Vector3 offset = Vector3.zero; // Position offset
    [SerializeField] private bool useHorizontalRotationOnly = false; // Ignore vertical rotation

    private Vector3 targetPosition;

    /// <summary>
    /// Updates canvas position and rotation each frame to follow the VR camera smoothly.
    /// </summary>
    void Update()
    {
        if (vrCamera == null || canvasTransform == null || !canvasTransform.gameObject.GetComponent<Canvas>().enabled) return;

        targetPosition = vrCamera.position + vrCamera.forward * distanceFromCamera + offset;

        canvasTransform.position = Vector3.Lerp(canvasTransform.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);

        if (useHorizontalRotationOnly)
        {
            Quaternion flatRotation = Quaternion.Euler(0f, vrCamera.eulerAngles.y, 0f);
            canvasTransform.rotation = Quaternion.Slerp(canvasTransform.rotation, flatRotation, Time.unscaledDeltaTime * smoothSpeed);
        }
        else
        {
            Quaternion targetRotation = vrCamera.rotation;
            canvasTransform.rotation = Quaternion.Slerp(canvasTransform.rotation, targetRotation, Time.unscaledDeltaTime * smoothSpeed);
        }
    }
}