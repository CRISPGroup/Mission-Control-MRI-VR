using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;

public class SceneSwitcher : MonoBehaviour
{

    public GameObject menuPauserObject; 

    // Function to load a different scene by name
    public void LoadSceneByName(string sceneName)
    {
        if (menuPauserObject != null)
        {
            menuPauserObject.GetComponent<MenuPauser>().ExitPause();
        }
        SceneManager.LoadScene(sceneName);
    }

    // Function to load a different scene by its build index
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
