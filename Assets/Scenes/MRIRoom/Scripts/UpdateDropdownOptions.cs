using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateDropdownOptions : MonoBehaviour
{
    [SerializeField] private Dropdown myDropdown;

    [Header("Options in english")]
    [SerializeField] private List<string> englishOptions;

    [Header("Options in french")]
    [SerializeField] private List<string> frenchOptions;

    private LanguageManager.Lang lastLang = LanguageManager.Lang.English;
    private int lastSelectedIndex = 0;

    private void Start()
    {
        UpdateDropdown();
    }

    private void OnDisable()
    {
        // Sauvegarder l'index s�lectionn� avant la d�sactivation
        if (myDropdown != null)
        {
            lastSelectedIndex = myDropdown.value;
        }
    }

    private void OnEnable()
    {
        // Update seulement si la langue a chang�
        if (LanguageManager.Instance.CurrentLang != lastLang)
        {
            UpdateDropdown();
        }
        else
        {
            // Restore la derni�re s�lection connue
            if (myDropdown != null && myDropdown.options.Count > 0)
            {
                myDropdown.value = Mathf.Clamp(lastSelectedIndex, 0, myDropdown.options.Count - 1);
                myDropdown.RefreshShownValue();
            }
        }
    }

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

            // Restaure la s�lection pr�c�dente si possible, sinon 0
            myDropdown.value = Mathf.Clamp(lastSelectedIndex, 0, options.Count - 1);
            myDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogWarning("No options defined for the current language.");
        }
    }

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

    // Cette m�thode doit �tre li�e au Dropdown.onValueChanged via l'inspecteur
    public void OnDropdownValueChanged(int newIndex)
    {
        lastSelectedIndex = newIndex;
    }
}
