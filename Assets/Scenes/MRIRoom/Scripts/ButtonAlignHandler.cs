using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAlignHandler : MonoBehaviour
{
    [System.Serializable]
    public class ButtonAction
    {
        public Button button;         // The button
        public UnityEvent onHold;     // Event invoked while button is held
        public UnityEvent onRelease;  // Event invoked when button is released
    }

    [SerializeField]
    private List<ButtonAction> buttonActions; // List of buttons and their UnityEvents

    [SerializeField] ControllerInputDetector controllerInputDetector;

    private Dictionary<Button, bool> buttonStateMap; // Map to track if buttons are being held

    private void Start()
    {
        buttonStateMap = new Dictionary<Button, bool>();

        foreach (var action in buttonActions)
        {
            if (action.button != null)
            {
                buttonStateMap[action.button] = false;

                // Add EventTrigger components for PointerDown and PointerUp
                AddEventTrigger(action.button, action);
            }
        }
    }

    private void Update()
    {
        // Call onHold event for all buttons currently being held
        foreach (var action in buttonActions)
        {
            if (buttonStateMap.ContainsKey(action.button) && buttonStateMap[action.button])
            {
                action.onHold?.Invoke();
            }
        }
    }

    private void AddEventTrigger(Button button, ButtonAction action)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Add PointerDown event
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((_) => OnButtonDown(action));
        trigger.triggers.Add(pointerDownEntry);

        // Add PointerUp event
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((_) => OnButtonUp(action));
        trigger.triggers.Add(pointerUpEntry);
    }

    private void OnButtonDown(ButtonAction action)
    {
        if (buttonStateMap.ContainsKey(action.button))
        {
            controllerInputDetector.SetHoldingEnabled(false);
            buttonStateMap[action.button] = true; // Mark the button as held
        }
    }

    private void OnButtonUp(ButtonAction action)
    {
        if (buttonStateMap.ContainsKey(action.button))
        {
            controllerInputDetector.SetHoldingEnabled(true);
            buttonStateMap[action.button] = false; // Mark the button as released
            action.onRelease?.Invoke(); // Call onRelease event

        }
    }
}