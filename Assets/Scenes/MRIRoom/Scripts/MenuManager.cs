using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages menu UI elements across scenes, enabling or disabling specific buttons or dropdowns
/// depending on the currently active scene.
/// </summary>
/// <remarks>
/// This script ensures consistent UI state management when transitioning between scenes such as
/// “OnMoon”, “MRIRoom”, or others. It automatically configures the availability of scenario selection
/// and settings options based on context.
/// <br/><br/>
/// Usage:
/// <list type="bullet">
/// <item>Attach this script to a persistent GameObject containing UI references.</item>
/// <item>Assign <see cref="selectScenario"/> and <see cref="moonTripSettings"/> in the Inspector.</item>
/// <item>Will automatically execute configuration logic in <see cref="OnSceneStart"/> at startup.</item>
/// </list>
/// </remarks>
public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the <see cref="MenuManager"/> accessible globally.
    /// </summary>
    public static MenuManager Instance;

    [Header("UI References")]
    [Tooltip("Main GameObject containing scenario selection buttons.")]
    public GameObject selectScenario;

    [Tooltip("Main GameObject containing Moon Trip settings and dropdowns.")]
    public GameObject moonTripSettings;

    /// <summary>
    /// Initializes the singleton instance and applies menu configuration for the current scene.
    /// </summary>
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            OnSceneStart();
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    /// <summary>
    /// Adjusts menu UI elements depending on the active scene name.
    /// </summary>
    private void OnSceneStart()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "OnMoon")
        {
            // Toggle scenario children visibility
            if(selectScenario != null)
            {
                selectScenario.transform.GetChild(0).gameObject.SetActive(false);
                selectScenario.transform.GetChild(1).gameObject.SetActive(true);
            }

            // Disable dropdown interactivity in Moon Trip settings
            if (moonTripSettings != null)
            {
                Dropdown dropdown = moonTripSettings.transform.GetChild(2).GetComponent<Dropdown>();
                if (dropdown != null)
                {
                    dropdown.interactable = false;
                }
            }

        }

        else if (currentScene.name != "MRIRoom")
        {
            if (selectScenario != null)
            {
                Button button = selectScenario.transform.GetChild(0).GetComponent<Button>();

                if (button != null)
                {
                    button.interactable = false;
                }
            }

            if (moonTripSettings != null)
            {
                Dropdown dropdown = moonTripSettings.transform.GetChild(2).GetComponent<Dropdown>();
                if (dropdown != null)
                {
                    dropdown.interactable = false;
                }
            }     
        }
    }

}
