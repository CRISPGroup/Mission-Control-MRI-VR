using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Custom handler for UI button interactions, managing press, hold, and release events.
/// Allows assigning UnityEvents for both continuous (hold) and discrete (release) actions,
/// while coordinating controller input through a ControllerInputDetector to avoid conflicts during interaction.
/// </summary>
public class CustomButtonInteractionHandler : MonoBehaviour
{
    /// <summary>
    /// Defines a button and its associated hold/release events.
    /// </summary>
    [System.Serializable]
    public class ButtonAction
    {
        [Tooltip("The UI button to monitor.")]
        public Button button;

        [Tooltip("Event invoked continuously while the button is held down.")]
        public UnityEvent onHold;

        [Tooltip("Event invoked once when the button is released.")]
        public UnityEvent onRelease;
    }

    [Tooltip("List of buttons and their associated hold/release events.")]
    [SerializeField] private List<ButtonAction> buttonActions;

    [Tooltip("Reference to the controller input detector managing input state.")]
    [SerializeField] ControllerInputDetector controllerInputDetector;

    private Dictionary<Button, bool> buttonStateMap; // Tracks whether each button is currently held

    /// <summary>
    /// Initializes the button state map and sets up pointer event triggers for each button.
    /// </summary>
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

    /// <summary>
    /// Invokes the <see cref="ButtonAction.onHold"/> event for any button currently being held down.
    /// </summary>
    private void Update()
    {
        foreach (var action in buttonActions)
        {
            if (buttonStateMap.ContainsKey(action.button) && buttonStateMap[action.button])
            {
                action.onHold?.Invoke();
            }
        }
    }

    /// <summary>
    /// Adds pointer down and pointer up event triggers to a button for detecting hold and release.
    /// </summary>
    /// <param name="button">The button to add triggers to.</param>
    /// <param name="action">The associated button action containing UnityEvents.</param>
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

    /// <summary>
    /// Called when a button is pressed down.
    /// Disables controller holding input and marks the button as held.
    /// </summary>
    /// <param name="action">The button action associated with the pressed button.</param>
    private void OnButtonDown(ButtonAction action)
    {
        if (buttonStateMap.ContainsKey(action.button))
        {
            controllerInputDetector.SetHoldingEnabled(false);
            buttonStateMap[action.button] = true;
        }
    }

    /// <summary>
    /// Called when a button is released.
    /// Re-enables controller holding input, resets the button state, and invokes the release event.
    /// </summary>
    /// <param name="action">The button action associated with the released button.</param>
    private void OnButtonUp(ButtonAction action)
    {
        if (buttonStateMap.ContainsKey(action.button))
        {
            controllerInputDetector.SetHoldingEnabled(true);
            buttonStateMap[action.button] = false;
            action.onRelease?.Invoke();

        }
    }
}