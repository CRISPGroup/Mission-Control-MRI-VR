using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the current language state across the application.
/// Supports persistence using <see cref="PlayerPrefs"/> and runtime updates triggered by UI elements.
/// </summary>
/// <remarks>
/// - Singleton-based manager to ensure one active instance at a time.<br/>
/// - Languages supported: English and French.<br/>
/// - Automatically loads and saves language preference using PlayerPrefs.<br/>
/// - Logs the source of the change (e.g., which button triggered it).<br/>
/// - Can be switched dynamically at runtime.
/// </remarks>
public class LanguageManager : MonoBehaviour
{
    /// <summary>
    /// Supported language options.
    /// </summary>
    public enum Lang { English, French }

    /// <summary>
    /// Global singleton instance for accessing the current language.
    /// </summary>
    public static LanguageManager Instance { get; private set; }

    [Header("Language Settings")]
    [Tooltip("Initial language set in the Inspector (used if no saved preference exists).")]
    [SerializeField] private Lang currentLangInspector = Lang.English;

    /// <summary>
    /// The currently active language.
    /// </summary>
    public Lang CurrentLang
    {
        get => currentLangInspector;
        private set => currentLangInspector = value;
    }

    private bool isInitialized = false;

    /// <summary>
    /// Initializes the LanguageManager and loads the saved preference if available.
    /// </summary>
    public void Init()
    {
        // Ensure singleton integrity
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (isInitialized) return;

        // Load saved language from PlayerPrefs
        string savedLang = PlayerPrefs.GetString("Language", "");
        if (!string.IsNullOrEmpty(savedLang))
        {
            ApplyLanguage(savedLang);
            Debug.Log($"[LanguageManager] Loaded saved language: {CurrentLang}");
        }

        isInitialized = true;
    }

    /// <summary>
    /// Changes the language and saves the selection to PlayerPrefs.
    /// </summary>
    /// <param name="newLang">Language name as a string ("English" or "French").</param>
    public void SetLanguage(string newLang)
    {
        // Identify the UI element or system that triggered the change (for debugging)
        string caller = EventSystem.current?.currentSelectedGameObject?.name ?? "Unknown";

        ApplyLanguage(newLang);
        PlayerPrefs.SetString("Language", newLang);
        PlayerPrefs.Save();

        Debug.Log($"[LanguageManager] Language set to: {CurrentLang} by {caller}");
    }

    /// <summary>
    /// Internal helper that safely converts a string to a <see cref="Lang"/> enum value.
    /// </summary>
    /// <param name="newLang">Language string to apply.</param>
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
    /// <summary>
    /// Automatically applies language changes when modified from the Unity Inspector during Play Mode.
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying && Instance == this)
        {
            SetLanguage(CurrentLang.ToString());
        }
    }
#endif
}
