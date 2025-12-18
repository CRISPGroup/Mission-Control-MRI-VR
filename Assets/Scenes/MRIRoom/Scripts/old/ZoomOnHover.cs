using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ZoomOnHover : MonoBehaviour
{
    public float zoomScale = 1.35f; // Factor for enlargement (e.g., 1.35 for a 35% increase)
    private Vector3 originalScale;
    private Coroutine zoomCoroutine;

    private void Start()
    {
        originalScale = transform.localScale; // Store the original scale
        //Debug.Log("Original Scale: " + originalScale);
    }

    private void OnEnable()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void OnDisable()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        //Debug.Log("Hover Entered");
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(SmoothZoom(true)); // Zoom in
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        //Debug.Log("Hover Exited");
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(SmoothZoom(false)); // Zoom out
    }

    private System.Collections.IEnumerator SmoothZoom(bool zoomIn)
    {
        //Debug.Log("SmoothZoom: " + (zoomIn ? "Zooming In" : "Zooming Out"));
        Vector3 targetScale = zoomIn ? originalScale * zoomScale : originalScale; // Full vector for precision
        Vector3 startScale = transform.localScale;
        float timeElapsed = 0f;
        float duration = 0.3f; // Duration of the zoom effect

        while (timeElapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is set accurately
        transform.localScale = targetScale;

        //Debug.Log("Zoom Complete. Final Scale: " + transform.localScale);
    }
}
