using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutomaticPauser : MonoBehaviour
{
    public List<AudioClip> audioClips;
    public List<AudioClip> audioClipsFR;

    public AudioSource audioSource;
    public MenuPauser menuPauser; // Reference to MenuPauser
    public GameObject pauseMovingCanvas;

    public float delayBetweenFeedbacks = 15f;
    public UnityEvent onLastClipFinished; // Unity Event for the last clip

    private int index = 0;
    private bool isPausing = false;
    private bool isPauseAutomatic = false;
    private bool isStoppedExternally = false;
    private Coroutine pauseRoutine;

    public void SetAudioIndex(int index)
    {
        this.index = index;
    }

    public void SetIsPauseAutomatic(UnityEngine.UI.Dropdown dropdown)
    {
        if (dropdown.value == 0)
        {
            isPauseAutomatic = false;
        }

        else if (dropdown.value == 1)
        {
            isPauseAutomatic = true;
        }

    }

    public void StartAutomaticPauser()
    {

        if (isPauseAutomatic && pauseRoutine == null)
        {
            pauseRoutine = StartCoroutine(PauseRoutine());
        }
    }


    public void SetIsPauseAutomatic(bool isPauseAutomatic)
    {
        this.isPauseAutomatic = isPauseAutomatic;
    }

    void DisplayMovingCanvasInFront()
    {
        //pauseMovingCanvas.SetActive(true);
        pauseMovingCanvas.GetComponent<Canvas>().enabled = true;
        pauseMovingCanvas.GetComponent<TextDisplayer>().enabled = true;
        if(index == 0)
            pauseMovingCanvas.GetComponent<TextDisplayer>().InitText();
        else
            pauseMovingCanvas.GetComponent<TextDisplayer>().DisplaySpecificSegment(index*2);

        //Debug.Log("displaying first segment inside AutomaticPauser (DisplayMovingCanvasInFront)");
        StartCoroutine(WaitXSeconds(5f));
    }

    IEnumerator WaitXSeconds(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        pauseMovingCanvas.GetComponent<TextDisplayer>().NextSegment();
        //Debug.Log("displaying second segment inside AutomaticPauser (WaitXSeconds)");
    }

    private IEnumerator PauseRoutine()
    {
        if (!isStoppedExternally && isPauseAutomatic)
        {

            var clips = GetCurrentAudioClips();

            if (clips.Count == 0 || index >= clips.Count)
            {
                isStoppedExternally = true;
            }
            else if (!isPausing)
            {
                yield return new WaitForSecondsRealtime(delayBetweenFeedbacks);

                if (!isPauseAutomatic) yield break;

                DisplayMovingCanvasInFront();
                PlayNextClip();
            }
        }
        pauseRoutine = null;
    }


    private void PlayNextClip()
    {
        var clips = GetCurrentAudioClips();

        if (clips.Count == 0)
        {
            return;
        }

        if(index >= clips.Count)
        {
            index = 0;
        }

        if (menuPauser != null && !menuPauser.GetInPause())
        {
            menuPauser.SetMenuAudioSource(audioSource);
            menuPauser.StartPause(true);
            audioSource.PlayOneShot(clips[index]);

            isPausing = true;

            index++;
        }
        else
        {
            return;
        }

    }

    public void StopCountdownExternally()
    {
        if (pauseRoutine != null)
        {
            isStoppedExternally = true;
            StopCoroutine(pauseRoutine);
            pauseRoutine = null;
        }
    }

    private void Update()
    {
        // Check if the AudioSource is still playing
        if (!audioSource.isPlaying)
        {
            isPausing = false;
            isStoppedExternally = false;
        }
    }

    private List<AudioClip> GetCurrentAudioClips()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? audioClipsFR : audioClips;
    }

}
