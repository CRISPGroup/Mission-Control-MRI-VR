using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    // R�f�rences aux GameObjects
    public GameObject selectScenario;
    public GameObject moonTripSettings;

    private void Start()
    {
        // Assurez-vous que l'instance est d�finie d�s le d�but
        if (Instance == null)
        {
            Instance = this;
            OnSceneStart();
        }
        else
        {
            Destroy(gameObject); // Assurez-vous qu'il n'y a qu'une seule instance de MenuManager
        }
    }

    private void OnSceneStart()
    {
        // V�rifiez quelle sc�ne est actuellement active
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "OnMoon")
        {
            // Dans la sc�ne OnMoon, d�sactivez le premier enfant et activez le second
            if(selectScenario != null)
            {
                selectScenario.transform.GetChild(0).gameObject.SetActive(false);
                selectScenario.transform.GetChild(1).gameObject.SetActive(true);
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

        else if (currentScene.name != "MRIRoom")
        {
            if (selectScenario != null)
            {
                Button button = selectScenario.transform.GetChild(0).GetComponent<Button>();

                // D�sactiver l'interactivit� du bouton
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
