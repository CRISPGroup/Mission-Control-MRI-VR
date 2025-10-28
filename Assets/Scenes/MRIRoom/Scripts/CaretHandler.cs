using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Replace le caret d’un TMP_InputField exactement à l’endroit du clic.
/// Compatible VR / UI (pas besoin de caméra).
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class CaretHandler : MonoBehaviour, IPointerDownHandler
{
    private TMP_InputField inputField;
    private TMP_Text textComponent;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        textComponent = inputField.textComponent;

        // Empêche la sélection complète du texte
        inputField.onFocusSelectAll = false;
        inputField.shouldHideSoftKeyboard = true;
        inputField.shouldHideMobileInput = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        inputField.ActivateInputField();

        // Force la mise à jour des infos de texte avant calcul
        textComponent.ForceMeshUpdate();

        // Position du clic dans le repère local du texte
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localClick))
            return;

        int nearestIndex = GetNearestCharacterIndex(localClick);
        inputField.caretPosition = nearestIndex;
        inputField.selectionAnchorPosition = nearestIndex;
        inputField.selectionFocusPosition = nearestIndex;
        inputField.ForceLabelUpdate();

        Debug.Log($"[CaretHandler] Caret déplacé à l'index {nearestIndex}");
        eventData.Use();
    }

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

            // Position du caractère dans le repère local du texte
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

        // Si clic après le dernier caractère
        if (localClick.x > textInfo.characterInfo[textInfo.characterCount - 1].topRight.x)
            nearestIndex = textInfo.characterCount;

        return nearestIndex;
    }
}
