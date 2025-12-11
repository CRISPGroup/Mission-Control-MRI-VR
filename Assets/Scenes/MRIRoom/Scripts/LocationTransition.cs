using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a specific location configuration in the scene,
/// including the transform reference and the objects to enable/disable when transitioning to this location.
/// </summary>
[System.Serializable]
public class LocationSetup
{
    [Tooltip("The target transform representing this location (position and rotation).")]
    public Transform locationPoint;

    [Tooltip("Objects that should be activated when entering this location.")]
    public List<GameObject> objectsToEnable;

    [Tooltip("Objects that should be deactivated when leaving this location.")]
    public List<GameObject> objectsToDisable;
}

/// <summary>
/// Manages transitions between predefined locations in a VR environment.
/// Each location includes a target Transform and lists of GameObjects to enable or disable.
/// Handles smooth scene transitions using a <see cref="FadeScreen"/> and keeps head/origin alignment consistent.
/// </summary>
public class LocationTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Reference to the FadeScreen component handling fade in/out effects.")]
    [SerializeField] private FadeScreen fadeScreen;

    [Tooltip("List of location setups defining all available destinations.")]
    [SerializeField] private List<LocationSetup> locations;

    [Tooltip("Transform representing the VR camera/head position.")]
    [SerializeField] private Transform head;

    [Tooltip("Transform of the XR Origin (the root of the VR rig).")]
    [SerializeField] private Transform origin;

    [Tooltip("Duration (in seconds) of the fade and transition between locations.")]
    [SerializeField] private float locationTransitionDuration = 2f;


    private int locationIndex = 0;
    private LocationSetup previousLocationSetup;
    private Quaternion savedOriginRotation;
    private Vector3 savedHeadForward;

    /// <summary>
    /// Initializes reference orientation at startup.
    /// </summary>
    public void Start()
    {
        //StartCoroutine(GoToFirstLocation(1));
        SaveReferenceOrientation();
    }

    /// <summary>
    /// Sets a custom duration for future location transitions.
    /// </summary>
    /// <param name="duration">Transition duration in seconds.</param>
    public void SetLocationTransitionDuration(float duration)
    {
        locationTransitionDuration = duration;
    }

    /// <summary>
    /// Saves the current origin rotation and forward direction of the head
    /// to maintain consistent alignment between locations.
    /// </summary>
    public void SaveReferenceOrientation()
    {
        savedOriginRotation = origin.rotation;

        Vector3 forward = head.forward;
        forward.y = 0;
        savedHeadForward = forward.normalized;
    }

    /// <summary>
    /// A specific method that handles vertical adjustment when colliding with the top of the scanner area.
    /// </summary>
    public void HandleScannerTopCollision()
    {
        Transform newTransform = origin;
        Vector3 newPosition = newTransform.position;
        newPosition.y = -.35f;
        origin.position = newPosition;
        newTransform.position = newPosition;
        //Recenter(newTransform);
    }

    /// <summary>
    /// A specific method that handles vertical adjustment when colliding with the bottom of the scanner area.
    /// </summary>
    public void HandleScannerBottomCollision()
    {
        Transform newTransform = origin;
        Vector3 newPosition = newTransform.position;
        newPosition.y = 0.175f;
        origin.position = newPosition;
        newTransform.position = newPosition;
        //Recenter(newTransform);
    }

    /// <summary>
    /// Recenters the XR Origin based on the head’s position and a target transform.
    /// Adjusts both rotation (around Y-axis) and position to realign the view.
    /// Compatible with laying down and standing experiences.
    /// </summary>
    /// <param name="target">Transform representing the desired target position and rotation.</param>
    public void Recenter(Transform target)
    {
        // Calculation of local offset (position + rotation of the head in the XR Origin)
        Matrix4x4 originToWorld = origin.localToWorldMatrix;
        Matrix4x4 worldToOrigin = originToWorld.inverse;

        Vector3 headLocalPos = worldToOrigin.MultiplyPoint(head.position);
        Quaternion headLocalRot = Quaternion.Inverse(origin.rotation) * head.rotation;

        // Target desired position and rotation
        Vector3 desiredHeadWorldPos = target.position;
        Quaternion desiredHeadWorldRot = target.rotation;

        // Calculate the overall rotation of the XR Origin
        Quaternion newOriginRot = desiredHeadWorldRot * Quaternion.Inverse(headLocalRot);

        // Correction: only the Y component of this rotation is retained
        Vector3 euler = newOriginRot.eulerAngles;
        newOriginRot = Quaternion.Euler(0, euler.y, 0);

        // Recalculate position with cleaned rotation
        Vector3 newOriginPos = desiredHeadWorldPos - newOriginRot * headLocalPos;

        // Apply the new layout to Origin
        origin.SetPositionAndRotation(newOriginPos, newOriginRot);

        SaveReferenceOrientation();
    }

    /// <summary>
    /// Recenters the XR Origin using the current active location as target.
    /// </summary
    public void Recenter()
    {
        Transform target = locations[locationIndex % locations.Count].locationPoint;
        Recenter(target);
    }

    /// <summary>
    /// Coroutine that waits before moving to the first location.
    /// </summary>
    /// <param name="waitTime">Delay before recentering, in seconds.</param>
    public IEnumerator GoToFirstLocation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Recenter();
    }

    /// <summary>
    /// Increments the internal location index (used for next-location transitions).
    /// </summary>
    public void IncrementLocation()
    {
        locationIndex++;
    }

    /// <summary>
    /// Sets the current location index manually, without triggering a transition.
    /// </summary>
    /// <param name="index">Target index within the location list.</param>
    public void SetLocationIndex(int index)
    {
        if (index < 0 || index >= locations.Count)
        {
            Debug.LogWarning($"Invalid location index: {index}");
            return;
        }

        previousLocationSetup = locations[locationIndex];

        locationIndex = index;
    }

    /// <summary>
    /// Moves the player to a specific indexed location with fade transition and object activation handling.
    /// </summary>
    /// <param name="index">Target index in the list of locations.</param>
    public void GoToSpecificLocation(int index)
    {
        if (index < 0 || index >= locations.Count)
        {
            Debug.LogWarning($"Invalid location index: {index}");
            return;
        }
        previousLocationSetup = locations[locationIndex];
        locationIndex = index;

        StartCoroutine(GoToLocation(locations[locationIndex]));
    }

    /// <summary>
    /// Moves the player to a specific location based on a Transform reference.
    /// </summary>
    /// <param name="target">Transform of the target location.</param>
    public void GoToSpecificLocation(Transform target)
    {
        LocationSetup setup = locations.Find(l => l.locationPoint == target);
        if (setup == null)
        {
            Debug.LogWarning($"No LocationSetup found for transform {target.name}");
            return;
        }

        previousLocationSetup = locations[locationIndex];
        locationIndex = locations.IndexOf(setup);

        StartCoroutine(GoToLocation(setup));
    }

    /// <summary>
    /// Moves to the next location in the list cyclically.
    /// </summary>
    public void GoToNextLocation()
    {
        previousLocationSetup = locations[locationIndex];
        locationIndex = (locationIndex + 1) % locations.Count;

        StartCoroutine(GoToLocation(locations[locationIndex]));
    }

    /// <summary>
    /// Re-centers the XR Origin to the current location without moving to a new one.
    /// </summary>
    public void ResetCurrentLocation()
    {
        Recenter();
    }

    /// <summary>
    /// Ends the current session, quitting play mode in Editor or exiting the application in a build.
    /// </summary>
    public void EndLocation()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
    }

    /// <summary>
    /// Coroutine that fades the screen, transitions to a new location,
    /// activates/deactivates objects, adjusts orientation and position, and fades back in.
    /// </summary>
    /// <param name="newSetup">Target location configuration.</param>
    public IEnumerator GoToLocation(LocationSetup newSetup)
    {
        Transform newLocation = newSetup.locationPoint;

        fadeScreen.SetFadeDuration(locationTransitionDuration);
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(locationTransitionDuration);

        // (Optional) Wait a short time (0.2 to 1 second) to ensure complete darkness.
        yield return new WaitForSeconds(0.5f);

        if (previousLocationSetup != null)
        {
            foreach (var go in previousLocationSetup.objectsToDisable)
                if (go) go.SetActive(false);
        }

        foreach (var go in newSetup.objectsToEnable)
            if (go) go.SetActive(true);

        // player's neutral direction in current's world
        Vector3 currentWorldNeutralDirection = origin.rotation * Quaternion.Inverse(savedOriginRotation) * savedHeadForward;
        currentWorldNeutralDirection.y = 0;
        currentWorldNeutralDirection.Normalize();

        // actual direction of the dest (where they are looking)
        Vector3 destinationForward = newLocation.forward;
        destinationForward.y = 0;
        destinationForward.Normalize();

        // calculate the angle between the two
        float angle = Vector3.SignedAngle(currentWorldNeutralDirection, destinationForward, Vector3.up);

        // apply rotation around the head
        origin.RotateAround(head.position, Vector3.up, angle);

        // position the rig so that the head is in the correct place
        Vector3 headToRigOffset = head.position - origin.position;
        Vector3 newOriginPosition = newLocation.position - headToRigOffset;
        origin.position = newOriginPosition;

        // Wait a little longer if necessary before the fade-in.
        yield return new WaitForSeconds(0.2f);

        fadeScreen.FadeIn();

        previousLocationSetup = newSetup;
    }

    /// <summary>
    /// Performs a fade-out before ending the session or quitting the application.
    /// </summary>
    /// <param name="fadeDuration">Duration of the fade-out before quitting.</param>
    public IEnumerator PerformEndLocation(float fadeDuration)
    {
        fadeScreen.SetFadeDuration(locationTransitionDuration);
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeDuration - 0.5f);
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
    }
}