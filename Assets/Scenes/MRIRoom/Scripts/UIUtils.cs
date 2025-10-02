using UnityEngine;
using UnityEngine.UI;

public class UIUtils : MonoBehaviour
{
    /// <summary>
    /// Simulates a click on a button calling onClick.Invoke()
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
