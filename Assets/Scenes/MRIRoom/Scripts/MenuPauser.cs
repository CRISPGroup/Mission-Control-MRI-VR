using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuPauser : MonoBehaviour
{

    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject menuCamera;
    [SerializeField] private AudioSource menuAudioSource;
    [SerializeField] private CanvasActivator canvasActivator;
    [SerializeField] private Button backToMainButton;

    [SerializeField] private AudioClip moonTripManualPauseAudio;
    [SerializeField] private AudioClip moonTripManualPauseAudioFr;

    [SerializeField] UnityEvent OnEnterPause;
    [SerializeField] UnityEvent OnFinishPause;

    private bool inPause = false;
    private CanvasGroup canvasGroup;
    private bool isMoonTrip = false; //If pause happens during moon trip

    private bool canShowMenu = true;

    void Start()
    {
        canvasGroup = menuCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is missing on the menuCanvas GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetMenuReady(bool ready)
    {
        canShowMenu = ready;
    }

    public void SetMenuAudioSource(AudioSource audioSource)
    {
        this.menuAudioSource = audioSource;
    }

    public void PerformPauseAction()
    {
        if (inPause)
        {
            ExitPause();
        }
        else
        {
            StartPause();
        }
    }

    public void ExitPause()
    {
        DesactivateMenuInFront();
        UnpauseGame();
        menuCamera.SetActive(false);
        OnFinishPause.Invoke();
        inPause = false;
        backToMainButton.onClick.Invoke();
    }

    public void DesactivateMenuInFront()
    {
        menuCanvas.GetComponent<Canvas>().enabled = false;
        //menuCanvas.SetActive(false);
    }

    /*
    public void StartPause()
    {
        OnEnterPause.Invoke();
        DisplayMenuInFront();
        PauseGame();
        menuCanvas.SetActive(true);
        StartCoroutine(ActivateMenuCameraDelayed());
    }
    */



    public void SetIsMoonTrip(bool isMoonTrip)
    {
        this.isMoonTrip = isMoonTrip;
    }


    public void StartPause(bool isAutomaticPause = false)
    {
        if (!canShowMenu)
        {
            return;
        }

        OnEnterPause.Invoke();

        PauseGame();
        if (isMoonTrip && !isAutomaticPause)
        {
            menuAudioSource.PlayOneShot(GetCurrentMoonClip());
        }

        if (!isAutomaticPause)
        {
            DisplayMenuInFront();
        }

        StartCoroutine(ActivateMenuCameraDelayed());
    }

    private IEnumerator ActivateMenuCameraDelayed()
    {
        // Attendre un frame pour s'assurer que tout est bien initialisé
        yield return null;
        menuCamera.SetActive(true);
        inPause = true;
    }

    public void DisplayMenuInFront()
    {
        Vector3 vHeadPos = Camera.main.transform.position;
        Vector3 vGazeDir = Camera.main.transform.forward;
        menuCanvas.transform.position = (vHeadPos + vGazeDir * 3.0f) + new Vector3(0.0f, -.40f, 0.0f);
        Vector3 vRot = Camera.main.transform.eulerAngles; vRot.z = 0;
        menuCanvas.transform.eulerAngles = vRot;

        //menuCanvas.SetActive(true);
        menuCanvas.GetComponent<Canvas>().enabled = true;
    }

    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            if (audioSource != menuAudioSource && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            audioSource.UnPause();
        }
    }

    public bool GetInPause()
    {
        return inPause;
    }

    private AudioClip GetCurrentMoonClip()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? moonTripManualPauseAudioFr : moonTripManualPauseAudio;
    }
}
