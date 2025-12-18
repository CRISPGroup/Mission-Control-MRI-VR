using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamically updates a <see cref="Dropdown"/>’s options.
/// </summary>
/// <remarks>
/// This script supports both English and French options and preserves the last selected index
/// when the dropdown is disabled or the language is changed.
/// </remarks>
public class UpdateDropdownOptions : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Dropdown myDropdown;

    [Header("Options in English")]
    [SerializeField] private List<string> englishOptions;

    [Header("Options in French")]
    [SerializeField] private List<string> frenchOptions;

    private LanguageManager.Lang lastLang = LanguageManager.Lang.English;
    private int lastSelectedIndex = 0;

    /// <summary>
    /// Unity lifecycle method — called on the first frame.
    /// Initializes the dropdown options based on the current language.
    /// </summary>
    private void Start()
    {
        UpdateDropdown();
    }

    /// <summary>
    /// Unity lifecycle method — called on the first frame.
    /// Initializes the dropdown options based on the dropdown value.
    /// </summary>
    private void OnDisable()
    {
        if (myDropdown != null)
        {
            lastSelectedIndex = myDropdown.value;
        }
    }

    /// <summary>
    /// Called when the GameObject becomes enabled again.
    /// Updates the dropdown only if the value has changed,
    /// otherwise restores the last known selection.
    /// </summary>
    private void OnEnable()
    {
        if (LanguageManager.Instance.CurrentLang != lastLang)
        {
            UpdateDropdown();
        }
        else
        {
            // Restore la dernière sélection connue
            if (myDropdown != null && myDropdown.options.Count > 0)
            {
                myDropdown.value = Mathf.Clamp(lastSelectedIndex, 0, myDropdown.options.Count - 1);
                myDropdown.RefreshShownValue();
            }
        }
    }

    /// <summary>
    /// Updates the dropdown options based on the current language
    /// and restores the previous selection if possible.
    /// </summary>
    public void UpdateDropdown()
    {
        if (myDropdown == null)
        {
            Debug.LogError("Dropdown not assigned.");
            return;
        }

        lastLang = LanguageManager.Instance.CurrentLang;

        List<string> options = GetCurrentLanguageOptions();

        myDropdown.ClearOptions();

        if (options != null && options.Count > 0)
        {
            myDropdown.AddOptions(options);

            // Restore previous selection if possible, 0 otherwise
            myDropdown.value = Mathf.Clamp(lastSelectedIndex, 0, options.Count - 1);
            myDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogWarning("No options defined for the current language.");
        }
    }

    /// <summary>
    /// Retrieves the list of options corresponding to the current language.
    /// </summary>
    /// <returns>
    /// A list of option strings in the selected language.
    /// </returns>
    private List<string> GetCurrentLanguageOptions()
    {
        switch (LanguageManager.Instance.CurrentLang)
        {
            case LanguageManager.Lang.French:
                return frenchOptions;

            case LanguageManager.Lang.English:
            default:
                return englishOptions;
        }
    }

    /// <summary>
    /// Called when the dropdown value changes.
    /// Should be linked to <see cref="Dropdown.onValueChanged"/> via the Inspector.
    /// </summary>
    /// <param name="newIndex">The newly selected option index.</param>
    public void OnDropdownValueChanged(int newIndex)
    {
        lastSelectedIndex = newIndex;
    }
}
