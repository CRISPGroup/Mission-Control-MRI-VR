using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Positions the caret of a <see cref="TMP_InputField"/> exactly at the user's click location.
/// Designed for VR and UI contexts — does not require a camera reference.
/// Prevents full text selection on focus and disables soft keyboard display.
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class CaretHandler : MonoBehaviour, IPointerDownHandler
{
    private TMP_InputField inputField;
    private TMP_Text textComponent;

    /// <summary>
    /// Initializes references and configures the input field to avoid full selection and unwanted keyboard display.
    /// </summary>
    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        textComponent = inputField.textComponent;

        inputField.onFocusSelectAll = false;
        inputField.shouldHideSoftKeyboard = true;
        inputField.shouldHideMobileInput = true;
    }

    /// <summary>
    /// Called when the user clicks or taps on the text field.
    /// Moves the caret to the closest character to the click position and consumes the event.
    /// </summary>
    /// <param name="eventData">Pointer event data containing click position.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        inputField.ActivateInputField();

        // Ensure text mesh is up to date before calculating caret position
        textComponent.ForceMeshUpdate();

        // Convert click position to local coordinates relative to the text
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localClick))
            return;

        int nearestIndex = GetNearestCharacterIndex(localClick);
        inputField.caretPosition = nearestIndex;
        inputField.selectionAnchorPosition = nearestIndex;
        inputField.selectionFocusPosition = nearestIndex;
        inputField.ForceLabelUpdate();

        //Debug.Log($"[CaretHandler] Caret déplacé à l'index {nearestIndex}");
        eventData.Use();
    }

    /// <summary>
    /// Finds the index of the character closest to the click position within the text.
    /// </summary>
    /// <param name="localClick">Local click coordinates relative to the text rectangle.</param>
    /// <returns>Index of the nearest character for caret placement.</returns>
    private int GetNearestCharacterIndex(Vector2 localClick)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;
        if (textInfo.characterCount == 0) return 0;

        int nearestIndex = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            float charX = textInfo.characterInfo[i].bottomLeft.x;
            float nextX = textInfo.characterInfo[i].topRight.x;
            float midX = (charX + nextX) * 0.5f;

            float dist = Mathf.Abs(localClick.x - midX);
            if (dist < minDist)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }

        // If the click is beyond the last character, move caret to end
        if (localClick.x > textInfo.characterInfo[textInfo.characterCount - 1].topRight.x)
            nearestIndex = textInfo.characterCount;

        return nearestIndex;
    }
}
