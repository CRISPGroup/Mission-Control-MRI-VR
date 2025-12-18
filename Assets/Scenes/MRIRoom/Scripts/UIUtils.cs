using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Utility class providing helper functions for UI interactions.
/// </summary>
public class UIUtils : MonoBehaviour
{
    /// <summary>
    /// Simulates a user click on a given <see cref="Button"/> by invoking its <see cref="Button.onClick"/> event.
    /// </summary>
    /// <param name="button">Target button.</param>
    public static void SimulateClick(Button button)
    {
        if (button != null && button.interactable)
        {
            button.onClick.Invoke();
        }
    }
}
