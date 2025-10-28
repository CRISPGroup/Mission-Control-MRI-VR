using System;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays and manages a <see cref="NonNativeKeyboard"/> for a <see cref="TMP_InputField"/> in XR.
/// Handles keyboard positioning, text synchronization, focus, and automatic closing when switching inputs.
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

    // --- Static event for global management ---
    public static event Action<KeyboardDisplayer> OnKeyboardOpenedGlobal;

    private bool isKeyboardOpen = false;
    private NonNativeKeyboard keyboard;

    private void OnEnable()
    {
        OnKeyboardOpenedGlobal += HandleOtherKeyboardOpened;
    }

    private void OnDisable()
    {
        OnKeyboardOpenedGlobal -= HandleOtherKeyboardOpened;
    }

    private void Start()
    {
        if (inputField == null)
        {
            Debug.LogError("InputField reference is missing.");
            return;
        }

        inputField.onSelect.AddListener(_ => OpenKeyboard());
    }

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

    private void HandleOtherKeyboardOpened(KeyboardDisplayer sender)
    {
        if (sender != this && isKeyboardOpen)
        {
            Debug.Log("[KeyboardDisplayer] Another input was selected, closing current keyboard.");
            CloseKeyboard();
        }
    }

    private void CloseKeyboard()
    {
        if (!isKeyboardOpen || keyboard == null)
            return;

        keyboard.Close();
        isKeyboardOpen = false;
    }

    private void PositionKeyboard(NonNativeKeyboard kbd)
    {
        Vector3 direction = positionSource.forward;
        direction.y = 0f;
        direction.Normalize();

        Vector3 targetPosition = positionSource.position + direction * distance + Vector3.up * verticalOffset;
        kbd.RepositionKeyboard(targetPosition);
    }

    private void SetCaretVisibility(bool visible)
    {
        inputField.customCaretColor = true;
        inputField.caretColor = visible ? Color.black : Color.white;
    }
}
