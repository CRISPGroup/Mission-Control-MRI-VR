using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public enum Lang { English, French }

    public static LanguageManager Instance { get; private set; }
    public Lang CurrentLang { get; private set; } = Lang.English;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject); // Optional, if you want it to persist across scenes

        string savedLang = PlayerPrefs.GetString("Language", "English");
        SetLanguage(savedLang);
    }

    public void SetLanguage(string newLang)
    {
        if (newLang == "French")
        {
            CurrentLang = Lang.French;
        }
        else if (newLang == "English")
        {
            CurrentLang = Lang.English;
        }
        else
        {
            Debug.LogWarning($"Unsupported language: {newLang}. Defaulting to English.");
            CurrentLang = Lang.English;
        }

        PlayerPrefs.SetString("Language", newLang);
        PlayerPrefs.Save();
        //Debug.Log($"Language set to: {CurrentLang}");
    }
}
