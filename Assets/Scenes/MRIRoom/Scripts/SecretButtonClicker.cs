using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecretButtonClicker : MonoBehaviour
{
    [Header("Buttons to activate secretely")]
    [SerializeField] private List<Button> leftButtons = new List<Button>();
    [SerializeField] private List<Button> rightButtons = new List<Button>();
    [SerializeField] private Transform head; // XR rig head transform

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


    private bool IsButtonNearWorld(Button btn, float maxDistance)
    {
        Vector3 buttonPosition = btn.transform.position;
        float distance = Vector3.Distance(head.position, buttonPosition);
        return distance < maxDistance;
    }


    /// <summary>
    /// Appelé lors du combo Y + Stick gauche
    /// </summary>
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
    /// Appelé lors du combo Y + Stick droit
    /// </summary>
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
