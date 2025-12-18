using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows triggering specific UI buttons "secretly" via controller input combos,
/// without requiring direct UI ray interaction.
/// </summary>
/// <remarks>
/// This component is designed for XR environments where hidden or debug actions
/// can be triggered using controller button combinations (e.g. Y + joystick left/right).  
/// It checks the visibility, interactivity, and proximity of buttons before invoking them,
/// preventing accidental activation of inactive or distant UI elements.
/// </remarks>
[DisallowMultipleComponent]
public class SecretButtonClicker : MonoBehaviour
{
    [Header("Buttons to activate secretly")]
    [Tooltip("List of buttons that can be triggered by the 'left combo' input (e.g., Y + Left Stick).")]
    [SerializeField] private List<Button> leftButtons = new List<Button>();

    [Tooltip("List of buttons that can be triggered by the 'right combo' input (e.g., Y + Right Stick).")]
    [SerializeField] private List<Button> rightButtons = new List<Button>();

    [Tooltip("Reference to the XR headset or camera (used to measure proximity to buttons).")]
    [SerializeField] private Transform head; // XR rig head transform

    /// <summary>
    /// Checks if a button is both active, visible, and part of an enabled canvas hierarchy.
    /// </summary>
    /// <param name="btn">Button to validate.</param>
    /// <returns>True if the button is active, visible, and interactable; false otherwise.</returns>
    private bool IsButtonActuallyVisible(Button btn)
    {
        if (btn == null || !btn.interactable || !btn.gameObject.activeInHierarchy)
            return false;

        Transform t = btn.transform;

        while (t != null)
        {
            Canvas canvas = t.GetComponent<Canvas>();
            if (canvas != null && !canvas.enabled)
                return false;

            if (!t.gameObject.activeInHierarchy)
                return false;

            t = t.parent;
        }

        return true;
    }

    /// <summary>
    /// Checks if a given button is within a certain world-space distance from the player's head.
    /// </summary>
    /// <param name="btn">Button to test.</param>
    /// <param name="maxDistance">Maximum allowed distance in meters.</param>
    /// <returns>True if the button is within range; false otherwise.</returns>
    private bool IsButtonNearWorld(Button btn, float maxDistance)
    {
        Vector3 buttonPosition = btn.transform.position;
        float distance = Vector3.Distance(head.position, buttonPosition);
        return distance < maxDistance;
    }

    /// <summary>
    /// Triggers the first valid "left" button when the combo (e.g. Y + left stick) is detected.
    /// </summary>
    /// <remarks>
    /// The button must be:
    /// <list type="bullet">
    /// <item><description>Active in hierarchy</description></item>
    /// <item><description>Interactable</description></item>
    /// <item><description>Visible (parent Canvas and GameObjects enabled)</description></item>
    /// <item><description>Within 3 meters of the player's head</description></item>
    /// </list>
    /// </remarks>
    public void TriggerLeftButton()
    {
        foreach (Button btn in leftButtons)
        {
            if (btn != null && IsButtonActuallyVisible(btn) && btn.isActiveAndEnabled && btn.interactable && IsButtonNearWorld(btn, 3f))
            {
                btn.onClick.Invoke();
                return;
            }
        }
    }

    /// <summary>
    /// Triggers the first valid "right" button when the combo (e.g. Y + right stick) is detected.
    /// </summary>
    /// <remarks>
    /// Similar to <see cref="TriggerLeftButton"/>, but operates on <see cref="rightButtons"/>.
    /// </remarks>
    public void TriggerRightButton()
    {
        foreach (Button btn in rightButtons)
        {
            if (btn != null && IsButtonActuallyVisible(btn) && btn.isActiveAndEnabled && btn.interactable && IsButtonNearWorld(btn, 3f))
            {
                btn.onClick.Invoke();
                //Debug.Log($"[SecretButtonClicker] Right button clicked: {btn.name}");
                return;
            }
        }
        //Debug.Log("[SecretButtonClicker] No active/interactable RIGHT button found.");
    }
}
