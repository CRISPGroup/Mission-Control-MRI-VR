using System;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays and manages a <see cref="NonNativeKeyboard"/> for a <see cref="TMP_InputField"/> in XR.
/// Handles keyboard positioning, text synchronization, and focus behavior to ensure text is retained
/// after pressing Enter.
/// </summary>
public class KeyboardDisplayer : MonoBehaviour
{
    [Header("Input Field")]
    [Tooltip("The TextMeshPro input field that this keyboard will populate.")]
    [SerializeField] private TMP_InputField inputField;

    [Header("Keyboard Positioning")]
    [Tooltip("The transform used as a reference point to position the keyboard.")]
    [SerializeField] private Transform positionSource;

    [Tooltip("Distance in front of the position source where the keyboard will appear.")]
    [SerializeField] private float distance = 0.5f;

    [Tooltip("Vertical offset relative to the position source.")]
    [SerializeField] private float verticalOffset = -0.5f;

    /// <summary>
    /// Registers the keyboard open handler when the input field is selected.
    /// </summary>
    private void Start()
    {
        if (inputField == null)
        {
            Debug.LogError("InputField reference is missing.");
            return;
        }

        inputField.onSelect.AddListener(_ => OpenKeyboard());
    }

    /// <summary>
    /// Opens the <see cref="NonNativeKeyboard"/>, attaches event handlers,
    /// and positions the keyboard relative to the specified <see cref="positionSource"/>.
    /// </summary>
    private void OpenKeyboard()
    {
        var keyboard = NonNativeKeyboard.Instance;
        keyboard.InputField = inputField;
        keyboard.PresentKeyboard(inputField.text);

        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnClosed += HandleKeyboardClosed;

        PositionKeyboard(keyboard);
        SetCaretVisibility(true);
    }

    /// <summary>
    /// Handles the keyboard text submission event.
    /// Updates the associated <see cref="TMP_InputField"/> with the submitted text
    /// and ensures the text remains visible after pressing Enter.
    /// </summary>
    /// <param name="sender">The keyboard instance that raised the event.</param>
    /// <param name="e">Event data (unused).</param>
    private void HandleTextSubmitted(object sender, EventArgs e)
    {
        if (sender is NonNativeKeyboard keyboard && keyboard.InputField != null)
        {
            inputField.text = keyboard.InputField.text;
            inputField.ForceLabelUpdate();
            inputField.DeactivateInputField();
        }
    }

    /// <summary>
    /// Handles keyboard closure events by hiding the caret and cleaning up event subscriptions.
    /// </summary>
    /// <param name="sender">The keyboard instance that raised the event.</param>
    /// <param name="e">Event data (unused).</param>
    private void HandleKeyboardClosed(object sender, EventArgs e)
    {
        SetCaretVisibility(false);

        var keyboard = NonNativeKeyboard.Instance;
        keyboard.OnClosed -= HandleKeyboardClosed;
        keyboard.OnTextSubmitted -= HandleTextSubmitted;
    }

    /// <summary>
    /// Positions the keyboard relative to the <see cref="positionSource"/> transform.
    /// </summary>
    /// <param name="keyboard">The <see cref="NonNativeKeyboard"/> instance to reposition.</param>
    private void PositionKeyboard(NonNativeKeyboard keyboard)
    {
        Vector3 direction = positionSource.forward;
        direction.y = 0f;
        direction.Normalize();

        Vector3 targetPosition = positionSource.position + direction * distance + Vector3.up * verticalOffset;
        keyboard.RepositionKeyboard(targetPosition);
    }

    /// <summary>
    /// Toggles the visibility of the text caret by adjusting its alpha channel.
    /// </summary>
    /// <param name="visible">True to make the caret visible, false to hide it.</param>
    private void SetCaretVisibility(bool visible)
    {
        inputField.customCaretColor = true;
        Color caretColor = inputField.caretColor;
        caretColor.a = visible ? 1f : 0f;
        inputField.caretColor = caretColor;
    }
}
