using UnityEngine;

public class ReticleController : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor; // The XRRayInteractor to follow

    [SerializeField]
    private GameObject crosshair; // The GameObject of the reticle

    [SerializeField]
    private Camera CameraFacing;

    [SerializeField]
    private Transform target;

    private Vector3 originalScale;
    private Vector3 currentScale;
    private Vector3 lastKnownPosition;
    private float lastKnownDistance;

    private bool wasGOenabled = false;
    private Vector3 lastLookDirection;

    void Start()
    {
        originalScale = transform.localScale;
        currentScale = originalScale;
        lastKnownPosition = transform.position;
        lastKnownDistance = CameraFacing.farClipPlane * 0.55f;
    }

    public void ResetReticleToCenter()
    {
        Vector3 forwardDirection = CameraFacing.transform.forward;
        rayInteractor.transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
    }

    public void AdjustReticleToTarget()
    {
        // Position the reticle on the target the first time the offset is calculated
        if (target != null && target.gameObject.activeSelf)
        {
            Vector3 directionToTarget = (target.position - rayInteractor.transform.position).normalized;
            rayInteractor.transform.rotation = Quaternion.LookRotation(directionToTarget);

        }
    }

    public void DisableIfGOEnabled()
    {
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            wasGOenabled = true;
        }
    }

    public void EnableIfGOEnabled()
    {
        if (wasGOenabled)
        {
            gameObject.SetActive(true);
            wasGOenabled = false;
        }
    }

    private void Update()
    {
        if (rayInteractor == null || crosshair == null)
        {
            Debug.LogError("RayInteractor or Crosshair is not assigned.");
            return;
        }

        // Obtain the position and direction of the ray
        Vector3 rayOrigin = rayInteractor.transform.position;
        Vector3 rayDirection = rayInteractor.transform.forward;

        // Raycast to detect collisions
        RaycastHit hitInfo;
        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo))
        {
            // If a collision is detected, smoothly transition to the new position
            lastKnownPosition = hitInfo.point;
            lastKnownDistance = hitInfo.distance;
        }
        else
        {
            // Otherwise, use the last known position, extending towards the far clip plane
            lastKnownPosition = rayOrigin + rayDirection * CameraFacing.farClipPlane * 0.95f;
            lastKnownDistance = CameraFacing.farClipPlane * 0.95f;
        }

        // Non-linear scaling factor to reduce "slipping" effect
        if (lastKnownDistance < 10)
        {
            lastKnownDistance *= 1 + 5 * Mathf.Exp(-lastKnownDistance);
        }


        // Smoothly adjust the scale to reduce abrupt changes
        float smoothFactor = 0.1f; // Adjust the smoothness as needed
        currentScale = Vector3.Lerp(currentScale, originalScale * lastKnownDistance, smoothFactor);
        transform.localScale = currentScale;

        // Smoothly transition to the new reticle position
        transform.position = Vector3.Lerp(transform.position, lastKnownPosition, smoothFactor);

        // NEW CODE

        // Get the parent's Z rotation and invert it locally
        float parentZ = rayInteractor.transform.eulerAngles.z;
        transform.localRotation = Quaternion.Euler(0, 180f, -parentZ); // 180° on Y to face the camera correctly

        /*
         *     OLD CODE   
            transform.LookAt(CameraFacing.transform.position);
            transform.Rotate(0f, 180f, 0f); // Ensures the reticle faces correctly
         */


    }
}
