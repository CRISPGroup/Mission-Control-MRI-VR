using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the progression of a UI Image with a "Filled" type to visually represent a loading or hold duration.
/// </summary>
/// <remarks>
/// - The fill amount increases smoothly over <see cref="loadingDuration"/> seconds.<br/>
/// - Can be triggered via <see cref="StartLoading"/> and reset with <see cref="ResetLoading"/>.<br/>
/// - Optionally tied to a <see cref="Canvas"/> GameObject to check visibility before updating.
/// </remarks>
public class LoadingFillImage : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Image with 'Filled' type used to display progress.")]
    public Image fillImage;

    [Tooltip("The Canvas that contains the loading indicator.")]
    public GameObject loadingCanvas;

    [Header("Timing Settings")]
    [Tooltip("Total duration (in seconds) of the loading animation.")]
    public float loadingDuration = 3f;   // Durée totale du "chargement"

    private float elapsedTime = 0f;
    private bool isLoading = false;

    /// <summary>
    /// Updates the fill progression each frame while loading is active and the canvas is visible.
    /// </summary>
    void Update()
    {
        if (!isLoading || !loadingCanvas.GetComponent<Canvas>().enabled) return;

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / loadingDuration);
        fillImage.fillAmount = progress;

        if (progress >= 1f)
        {
            isLoading = false;
            // Call any completion events or methods here if needed (e.g., OnLoadingComplete()).
        }
    }

    /// <summary>
    /// Sets a custom duration for the loading process.
    /// </summary>
    /// <param name="duration">The desired duration (in seconds).</param>
    public void SetLoadingDuration(float duration)
    {
        this.loadingDuration = duration;
    }

    /// <summary>
    /// Starts the loading progression and resets the fill amount.
    /// </summary>
    /// <param name="duration">Optional custom duration for this loading sequence.</param>
    public void StartLoading(float duration)
    {
        loadingDuration = duration;
        elapsedTime = 0f;
        fillImage.fillAmount = 0f;
        isLoading = true;
    }

    /// <summary>
    /// Resets the loading bar immediately to 0%.
    /// </summary>
    public void ResetLoading()
    {
        isLoading = false;
        elapsedTime = 0f;
        fillImage.fillAmount = 0f;
    }
}
