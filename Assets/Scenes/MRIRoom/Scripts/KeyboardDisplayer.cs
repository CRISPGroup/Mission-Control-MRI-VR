using System;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays and manages a <see cref="NonNativeKeyboard"/> for a <see cref="TMP_InputField"/> in XR.
/// </summary>
/// <remarks>
/// - Automatically opens the keyboard when the associated input field is selected.<br/>
/// - Positions the keyboard in front of a reference transform (typically the camera or a hand).<br/>
/// - Closes automatically when another <see cref="KeyboardDisplayer"/> opens.<br/>
/// - Syncs text and caret state with the TMP input field.<br/>
/// - Useful for VR/AR input when using TextMeshPro fields without a physical keyboard.
/// </remarks>
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
    /// Static event fired globally whenever a keyboard is opened, so others can close.
    /// </summary>
    public static event Action<KeyboardDisplayer> OnKeyboardOpenedGlobal;

    private bool isKeyboardOpen = false;
    private NonNativeKeyboard keyboard;

    /// <summary>
    /// Subscribes to global keyboard events when this component is enabled.
    /// </summary>
    private void OnEnable()
    {
        OnKeyboardOpenedGlobal += HandleOtherKeyboardOpened;
    }

    /// <summary>
    /// Unsubscribes from global events to prevent memory leaks or invalid callbacks when disabled.
    /// </summary>
    private void OnDisable()
    {
        OnKeyboardOpenedGlobal -= HandleOtherKeyboardOpened;
    }

    /// <summary>
    /// Initializes keyboard behavior by linking it to the assigned input field.
    /// Automatically sets up the listener to open the XR keyboard on selection.
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
    /// Opens the non-native keyboard and attaches listeners for text input and close events.
    /// </summary>
    private void OpenKeyboard()
    {
        // Close all others
        OnKeyboardOpenedGlobal?.Invoke(this);

        keyboard = NonNativeKeyboard.Instance;
        keyboard.InputField = inputField;
        keyboard.PresentKeyboard(inputField.text);

        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnClosed += HandleKeyboardClosed;

        PositionKeyboard(keyboard);
        SetCaretVisibility(true);
        isKeyboardOpen = true;
    }

    /// <summary>
    /// Handles text submission (e.g., when the user presses Enter) and closes the keyboard.
    /// </summary>
    private void HandleTextSubmitted(object sender, EventArgs e)
    {
        if (sender is NonNativeKeyboard k && k.InputField != null)
        {
            inputField.text = k.InputField.text;
            inputField.ForceLabelUpdate();
            inputField.ActivateInputField();
            inputField.Select();
        }

        CloseKeyboard(); // behave like Enter -> close
    }

    /// <summary>
    /// Called when the keyboard is closed, cleaning up listeners and caret visibility.
    /// </summary>
    private void HandleKeyboardClosed(object sender, EventArgs e)
    {
        SetCaretVisibility(false);
        isKeyboardOpen = false;

        if (keyboard != null)
        {
            keyboard.OnClosed -= HandleKeyboardClosed;
            keyboard.OnTextSubmitted -= HandleTextSubmitted;
        }
    }

    /// <summary>
    /// Closes this keyboard if another input field opens a new one.
    /// </summary>
    private void HandleOtherKeyboardOpened(KeyboardDisplayer sender)
    {
        if (sender != this && isKeyboardOpen)
        {
            Debug.Log("[KeyboardDisplayer] Another input was selected, closing current keyboard.");
            CloseKeyboard();
        }
    }

    /// <summary>
    /// Closes the currently open keyboard (if any).
    /// </summary>
    private void CloseKeyboard()
    {
        if (!isKeyboardOpen || keyboard == null)
            return;

        keyboard.Close();
        isKeyboardOpen = false;
    }

    /// <summary>
    /// Positions the keyboard relative to the given reference transform.
    /// </summary>
    private void PositionKeyboard(NonNativeKeyboard kbd)
    {
        Vector3 direction = positionSource.forward;
        direction.y = 0f;
        direction.Normalize();

        Vector3 targetPosition = positionSource.position + direction * distance + Vector3.up * verticalOffset;
        kbd.RepositionKeyboard(targetPosition);
    }

    /// <summary>
    /// Toggles the caret visibility to make typing feedback clearer.
    /// </summary>
    private void SetCaretVisibility(bool visible)
    {
        inputField.customCaretColor = true;
        inputField.caretColor = visible ? Color.black : Color.white;
    }
}
