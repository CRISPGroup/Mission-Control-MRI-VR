using UnityEngine;
using UnityEngine.EventSystems;

public class LanguageManager : MonoBehaviour
{
    public enum Lang { English, French }

    public static LanguageManager Instance { get; private set; }

    [SerializeField] private Lang currentLangInspector = Lang.English;
    public Lang CurrentLang
    {
        get => currentLangInspector;
        private set => currentLangInspector = value;
    }

    private bool isInitialized = false;

    public void Init()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Si déjà initialisé, ne rien refaire
        if (isInitialized) return;

        // Si PlayerPrefs contient déjà une langue: on la charge
        string savedLang = PlayerPrefs.GetString("Language", "");
        if (!string.IsNullOrEmpty(savedLang))
        {
            ApplyLanguage(savedLang);
            Debug.Log($"[LanguageManager] Loaded saved language: {CurrentLang}");
        }

        isInitialized = true;
    }

    public void SetLanguage(string newLang)
    {
        // Récupère qui a déclenché le changement (utile pour debug)
        string caller = EventSystem.current?.currentSelectedGameObject?.name ?? "Unknown";

        ApplyLanguage(newLang);
        PlayerPrefs.SetString("Language", newLang);
        PlayerPrefs.Save();

        Debug.Log($"[LanguageManager] Language set to: {CurrentLang} by {caller}");
    }

    private void ApplyLanguage(string newLang)
    {
        if (newLang == "French")
            CurrentLang = Lang.French;
        else if (newLang == "English")
            CurrentLang = Lang.English;
        else
        {
            Debug.LogWarning($"[LanguageManager] Unsupported language: {newLang}. Defaulting to English.");
            CurrentLang = Lang.English;
        }
    }

#if UNITY_EDITOR
    // Si on change la langue manuellement dans l’inspector
    private void OnValidate()
    {
        if (Application.isPlaying && Instance == this)
        {
            SetLanguage(CurrentLang.ToString());
        }
    }
#endif
}
