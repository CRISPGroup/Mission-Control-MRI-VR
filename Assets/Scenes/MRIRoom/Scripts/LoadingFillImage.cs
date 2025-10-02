using UnityEngine;
using UnityEngine.UI;

public class LoadingFillImage : MonoBehaviour
{
    public Image fillImage;              // Image UI avec type "Filled"
    public float loadingDuration = 3f;   // Durée totale du "chargement"
    public GameObject loadingCanvas;

    private float elapsedTime = 0f;
    private bool isLoading = false;

    public void SetLoadingDuration(float duration)
    {
        this.loadingDuration = duration;
    }

    void Update()
    {
        if (!isLoading || !loadingCanvas.GetComponent<Canvas>().enabled) return;

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / loadingDuration);
        fillImage.fillAmount = progress;

        if (progress >= 1f)
        {
            isLoading = false;
            //OnLoadingComplete();
        }
    }

    public void StartLoading(float duration)
    {
        loadingDuration = duration;
        elapsedTime = 0f;
        fillImage.fillAmount = 0f;
        isLoading = true;
    }

    public void ResetLoading()
    {
        isLoading = false;
        elapsedTime = 0f;
        fillImage.fillAmount = 0f;
    }

    private void OnLoadingComplete()
    {
        //Debug.Log("Loading finished!");
        // Tu peux appeler ici une animation, un son, un callback, etc.
    }
}
