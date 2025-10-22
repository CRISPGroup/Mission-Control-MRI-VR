using UnityEngine;

public class AlignCanvasWithView : MonoBehaviour
{
    [SerializeField] private Transform vrCamera;
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float maxTilt = 70f; // limite d’inclinaison pour éviter le retournement
    [SerializeField] bool useSmartPositioning = false;

    private Vector3 targetPosition;

    void Update()
    {
        if (!vrCamera || !canvasTransform || !canvasTransform.gameObject.activeInHierarchy) return;

        if (useSmartPositioning)
        {
            // --- POSITION ---
            targetPosition = vrCamera.position + vrCamera.forward * distanceFromCamera + offset;
            canvasTransform.position = Vector3.Lerp(
                canvasTransform.position,
                targetPosition,
                Time.unscaledDeltaTime * smoothSpeed
            );

            // --- ROTATION ---
            Vector3 euler = vrCamera.eulerAngles;

            // Normaliser les angles
            euler.x = NormalizeAngle(euler.x);
            euler.x = Mathf.Clamp(euler.x, -maxTilt, maxTilt);

            // Ne pas toucher au roll (z)
            // Garder le yaw et le roll naturels de la caméra
            Quaternion limitedPitch = Quaternion.Euler(euler.x, vrCamera.eulerAngles.y, vrCamera.eulerAngles.z);

            canvasTransform.rotation = Quaternion.Slerp(
                canvasTransform.rotation,
                limitedPitch,
                Time.unscaledDeltaTime * smoothSpeed
            );
        }

        else
        {
            targetPosition = vrCamera.position + vrCamera.forward * distanceFromCamera + offset;

            canvasTransform.position = Vector3.Lerp(canvasTransform.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);

            Quaternion targetRotation = vrCamera.rotation;
            canvasTransform.rotation = Quaternion.Slerp(canvasTransform.rotation, targetRotation, Time.unscaledDeltaTime * smoothSpeed);
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
