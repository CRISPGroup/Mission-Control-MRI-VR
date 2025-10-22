using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignCanvasWithView : MonoBehaviour
{
    [SerializeField] private Transform vrCamera; // La caméra ou le casque VR
    [SerializeField] private Transform canvasTransform; // Le Transform du Canvas à aligner
    [SerializeField] private float smoothSpeed = 5f; // Vitesse de glissement
    [SerializeField] private float distanceFromCamera = 2f; // Distance du Canvas par rapport à la caméra
    [SerializeField] private Vector3 offset = Vector3.zero; // Décalage personnalisé (optionnel)
    [SerializeField] private bool useHorizontalRotationOnly = false; // Utiliser une rotation fixe (optionnel)

    private Vector3 targetPosition;

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