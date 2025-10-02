using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MoonTripDurationController : MonoBehaviour
{
    [Header("UI References")]
    public Button LButton;
    public Button RButton;
    public TextMeshProUGUI durationText;

    [Header("Duration (in seconds)")]
    public int minDuration = 120;
    public int maxDuration = 1800;
    public int step = 60;

    public MoonMovement moonMovement;

    private int currentDuration;

    void Start()
    {
        currentDuration = 180; // Valeur initiale : 3 minutes

        // Lier les boutons
        LButton.onClick.AddListener(DecreaseDuration);
        RButton.onClick.AddListener(IncreaseDuration);

        UpdateDisplay();
    }

    void DecreaseDuration()
    {
        if (currentDuration > minDuration)
        {
            currentDuration -= step;
            UpdateDisplay();
        }
    }

    void IncreaseDuration()
    {
        if (currentDuration < maxDuration)
        {
            currentDuration += step;
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        int minutes = currentDuration / 60;
        durationText.text = $"{minutes} min.";

        // Appel de ta fonction avec la durée en secondes
        moonMovement.SetDuration(currentDuration);
    }
}
