using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    [Tooltip("Optionnel : référence à l’objet qui gère le menu pause")]
    public GameObject menuPauserObject;
    public FadeScreen fadeScreen;

    // Charger une scène par nom
    public void LoadSceneByName(string sceneName)
    {
        if (menuPauserObject != null)
            menuPauserObject.GetComponent<MenuPauser>().ExitPause();

        StartCoroutine(LoadSceneAsyncByName(sceneName));
    }

    // Charger une scène par index
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (menuPauserObject != null)
            menuPauserObject.GetComponent<MenuPauser>().ExitPause();

        StartCoroutine(LoadSceneAsyncByIndex(sceneIndex));
    }

    // Coroutine : chargement asynchrone par nom
    private IEnumerator LoadSceneAsyncByName(string sceneName)
    {
        fadeScreen.SetFadeDuration(1f);
        fadeScreen.FadeOut();
        yield return new WaitForSecondsRealtime(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        // (Optionnel) petit délai pour fluidifier le fade noir
        yield return new WaitForSecondsRealtime(0.2f);

        asyncLoad.allowSceneActivation = true;

    }

    // Coroutine : chargement asynchrone par index
    private IEnumerator LoadSceneAsyncByIndex(int sceneIndex)
    {
        fadeScreen.SetFadeDuration(1f);
        fadeScreen.FadeOut();
        yield return new WaitForSecondsRealtime(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        // (Optionnel)
        yield return new WaitForSecondsRealtime(0.2f);

        asyncLoad.allowSceneActivation = true;
    }
}
