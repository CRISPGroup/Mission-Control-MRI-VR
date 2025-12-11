using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controls the configurable duration of the moon trip via UI buttons,
/// updates the display text, and synchronizes the duration with <see cref="MoonMovement"/>.
/// </summary>
/// <remarks>
/// This component allows the user to adjust the total duration of the moon’s movement
/// (in seconds) within a specified range using increment/decrement buttons.
/// <br/><br/>
/// It automatically updates the text display and applies the new value to
/// the linked <see cref="MoonMovement"/> instance.
/// </remarks>
public class MoonTripDurationController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Button to decrease the total duration.")]
    public Button LButton;

    [Tooltip("Button to increase the total duration.")]
    public Button RButton;

    [Tooltip("Text element showing the current duration in minutes.")]
    public TextMeshProUGUI durationText;

    [Header("Duration (in seconds)")]
    [Tooltip("Minimum allowed duration for the moon trip.")]
    public int minDuration = 120;

    [Tooltip("Maximum allowed duration for the moon trip.")]
    public int maxDuration = 1800;

    [Tooltip("Increment/decrement step in seconds.")]
    public int step = 60;

    [Header("Linked Components")]
    [Tooltip("Reference to the MoonMovement script that will receive duration updates.")]
    public MoonMovement moonMovement;

    private int currentDuration;

    /// <summary>
    /// Initializes button listeners and sets the default duration display.
    /// </summary>
    void Start()
    {
        currentDuration = 180; // Default: 3 minutes

        LButton.onClick.AddListener(DecreaseDuration);
        RButton.onClick.AddListener(IncreaseDuration);

        UpdateDisplay();
    }

    /// <summary>
    /// Decreases the total duration by one step if above the minimum.
    /// </summary>
    void DecreaseDuration()
    {
        if (currentDuration > minDuration)
        {
            currentDuration -= step;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Increases the total duration by one step if below the maximum.
    /// </summary>
    void IncreaseDuration()
    {
        if (currentDuration < maxDuration)
        {
            currentDuration += step;
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Updates the on-screen duration text and applies the value to <see cref="moonMovement"/>.
    /// </summary>
    /// <remarks>
    /// The duration is displayed in minutes, but passed to the <see cref="MoonMovement"/> script in seconds.
    /// </remarks>
    void UpdateDisplay()
    {
        int minutes = currentDuration / 60;
        durationText.text = $"{minutes} min.";

        // Sync with MoonMovement
        moonMovement.SetDuration(currentDuration);
    }
}
