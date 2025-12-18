using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles scene transitions with optional fade effects and pause cleanup.
/// </summary>
/// <remarks>
/// This script provides asynchronous scene loading with fade in/out support,
/// ensuring smooth transitions without freezing the main thread.  
/// It also interacts with the <see cref="MenuPauser"/> component to exit pause mode before switching scenes.
/// </remarks>
[DisallowMultipleComponent]
public class SceneSwitcher : MonoBehaviour
{
    [Header("Scene Transition References")]
    [Tooltip("Optional reference to the object handling pause functionality.")]
    public GameObject menuPauserObject;

    [Tooltip("FadeScreen component used to fade in/out during scene transitions.")]
    public FadeScreen fadeScreen;

    /// <summary>
    /// Initiates a scene load by name, exiting pause mode if active.
    /// </summary>
    /// <param name="sceneName">The name of the target scene to load.</param>
    /// <example>
    /// Example usage:
    /// <code>
    /// SceneSwitcher.Instance.LoadSceneByName("MainMenu");
    /// </code>
    /// </example>
    public void LoadSceneByName(string sceneName)
    {
        if (menuPauserObject != null)
            menuPauserObject.GetComponent<MenuPauser>().ExitPause();

        StartCoroutine(LoadSceneAsyncByName(sceneName));
    }

    /// <summary>
    /// Initiates a scene load by build index, exiting pause mode if active.
    /// </summary>
    /// <param name="sceneIndex">The index of the scene in Build Settings.</param>
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (menuPauserObject != null)
            menuPauserObject.GetComponent<MenuPauser>().ExitPause();

        StartCoroutine(LoadSceneAsyncByIndex(sceneIndex));
    }

    /// <summary>
    /// Asynchronously loads a scene by name with a fade-out transition.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    /// <returns>Coroutine that performs the transition.</returns>
    /// <remarks>
    /// The scene is preloaded until asyncLoad.progress reaches 0.9f,
    /// then activated after the fade-out completes.  
    /// Uses <see cref="WaitForSecondsRealtime"/> to remain unaffected by Time.timeScale.
    /// </remarks>
    private IEnumerator LoadSceneAsyncByName(string sceneName)
    {
        fadeScreen.SetFadeDuration(1f);
        fadeScreen.FadeOut();
        yield return new WaitForSecondsRealtime(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        // Optional smooth delay
        yield return new WaitForSecondsRealtime(0.2f);

        asyncLoad.allowSceneActivation = true;

    }

    /// <summary>
    /// Asynchronously loads a scene by index with a fade-out transition.
    /// </summary>
    /// <param name="sceneIndex">The build index of the target scene.</param>
    /// <returns>Coroutine that performs the transition.</returns>
    private IEnumerator LoadSceneAsyncByIndex(int sceneIndex)
    {
        fadeScreen.SetFadeDuration(1f);
        fadeScreen.FadeOut();
        yield return new WaitForSecondsRealtime(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        yield return new WaitForSecondsRealtime(0.2f);

        asyncLoad.allowSceneActivation = true;
    }
}
