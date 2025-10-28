using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CountdownAudioPlayer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips;
    [SerializeField] private List<AudioClip> audioClipsFR;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] UnityEvent OnFinishPlayback;
    [SerializeField] private MoonMovement moonMovementScript;

    private Coroutine countdownCoroutine;
    private Coroutine audioLoopCoroutine;
    private float countdownTime = 30f;
    private bool isStoppedExternally = false;
    private int currentClipIndex = 0;

    private bool enableAutoIncrement = false;
    private float autoIncrementInterval = 0f;
    private float nextAutoIncrementTime = 0f;

    private float tripDuration = 0f;

    private bool replayAudios = false;

    private bool holdRoutine = false;

    private List<int> scheduledIndexes = new List<int>();
    private int scheduledPointer = 0;

    bool playing;

    void Start()
    {

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is not assigned or found!");
            }
        }
    }

    public void SetReplayAudios(bool replayAudios)
    {
        this.replayAudios = replayAudios;
    }

    public void SetHoldRoutine(bool holdRoutine)
    {
        this.holdRoutine = holdRoutine;
    }

    public void SetCurrentClipIndex(int clipIndex)
    {
        this.currentClipIndex = clipIndex;
    }

    public void SetCountDownTimer(float countDownTime)
    {
        this.countdownTime = countDownTime;
    }

    /*
    public void PlayNextClipAfterCountdownUntilStop(int clipIndexStart)
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        currentClipIndex = clipIndexStart;
        countdownCoroutine = StartCoroutine(CountdownRoutineUntilStop());
    }
    */


    public void StopCountdownExternally()
    {
        if (countdownCoroutine != null)
        {
            isStoppedExternally = true;
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (audioLoopCoroutine != null)
        {
            StopCoroutine(audioLoopCoroutine);
            audioLoopCoroutine = null;
        }

    }
    /*
    private void PlayNextAudioClip()
    {
        if (audioClips != null && audioClips.Count > 0)
        {
            if (audioSource != null)
            {
                currentClipIndex = (currentClipIndex + 1) % audioClips.Count;
                audioSource.clip = audioClips[currentClipIndex];
                audioSource.Play();
                playing = true;
            }
        }
    }
    */

    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            playing = false;
            //OnFinishPlayback.Invoke();
        }
    }

    public void PlaySpecificClipAfterCountdown(int clipIndex)
    {
        var clips = GetCurrentAudioClips();
        if (clipIndex < 0 || clipIndex >= clips.Count)
        {
            return;
        }

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = StartCoroutine(PlaySpecificClipAfterCountdownRoutine(clipIndex));
    }

    private IEnumerator PlaySpecificClipAfterCountdownRoutine(int clipIndex)
    {
        float timer = countdownTime;
        isStoppedExternally = false;

        while (timer > 0f)
        {
            yield return new WaitForSeconds(1f);
            timer--;

            if (isStoppedExternally)
            {
                countdownCoroutine = null;
                yield break;
            }
        }

        if (!isStoppedExternally)
        {
            var clips = GetCurrentAudioClips();
            audioSource.clip = clips[clipIndex];
            audioSource.Play();
            playing = true;
        }
    }

    public void PlaySpecificClipInLoopAfterCountdown(int clipIndex)
    {
        var clips = GetCurrentAudioClips();
        if (clipIndex < 0 || clipIndex >= clips.Count)
        {
            return;
        }

        isStoppedExternally = false;
        //Debug.Log("Before coroutine");

        if (countdownCoroutine != null)
        {
            StopCountdownExternally();
        }

        countdownCoroutine = StartCoroutine(PlaySpecificClipInLoopAfterCountdownRoutine(clipIndex));
    }

    private IEnumerator PlaySpecificClipInLoopAfterCountdownRoutine(int clipIndex)
    {
        //Debug.Log("Started coroutine");
        while (!isStoppedExternally)
        {
            // Attendre que le clip se termine
            yield return new WaitWhile(() => audioSource.isPlaying);

            // Attendre 30 secondes
            yield return new WaitForSeconds(countdownTime);

            if (!isStoppedExternally)
            {
                var clips = GetCurrentAudioClips();
                audioSource.clip = clips[clipIndex];
                audioSource.Play();
                playing = true;
            }
        }
        countdownCoroutine = null;
    }

    public void PlayNextClipAfterCountdown(bool waitForPreviousClipToFinish = true)
    {
        //Debug.Log("Before coroutine");
        isStoppedExternally = false;
        if (countdownCoroutine != null)
        {
            StopCountdownExternally();
        }

        countdownCoroutine = StartCoroutine(CountdownRoutine(waitForPreviousClipToFinish));
    }

    // holdRoutine may be deleted, I thought I had a bug where the routine restarted but it's just because I tested without putting the headset on my face

    private IEnumerator CountdownRoutine(bool waitForPreviousClipToFinish)
    {
        //Debug.Log("Started coroutine");
        if (!isStoppedExternally && !holdRoutine)
        {
            if(waitForPreviousClipToFinish)
            {
                // Attendre que le clip se termine
                yield return new WaitWhile(() => audioSource.isPlaying);
            }

            yield return new WaitForSeconds(countdownTime);

            var clips = GetCurrentAudioClips();

            if (currentClipIndex >= clips.Count && !replayAudios)
            {
                yield break;
            }

            if (currentClipIndex >= clips.Count && replayAudios)
            {
                currentClipIndex = 0;
            }

            if (!isStoppedExternally)
            {
                //Debug.Log("is playing: " + audioClips[currentClipIndex] + "at index: " + currentClipIndex);
                audioSource.clip = clips[currentClipIndex];
                audioSource.Play();
                playing = true;
                currentClipIndex = currentClipIndex + 1;
            }
        }
        //Debug.Log("Was stopped externally");
        countdownCoroutine = null;
    }

    public void PlayNextClipAfterCountdownUntilStop(bool waitForPreviousClipToFinish = true)
    {
        //Debug.Log("Before coroutine");
        isStoppedExternally = false;
        if (countdownCoroutine != null)
        {
            StopCountdownExternally();
        }

        countdownCoroutine = StartCoroutine(CountdownRoutineUntilStop(waitForPreviousClipToFinish));
    }

    private IEnumerator CountdownRoutineUntilStop(bool waitForPreviousClipToFinish)
    {
        while (!isStoppedExternally)
        {
            if (waitForPreviousClipToFinish){
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            
            yield return new WaitForSeconds(countdownTime);

            var clips = GetCurrentAudioClips();

            if (currentClipIndex >= clips.Count && !replayAudios)
            {
                yield break;
            }

            if (currentClipIndex >= clips.Count && replayAudios)
            {
                currentClipIndex = 0;
            }

            if (!isStoppedExternally)
            {
                //Debug.Log("is playing: " + audioClips[currentClipIndex] + " at index: " + currentClipIndex);
                audioSource.clip = clips[currentClipIndex];
                audioSource.Play();
                playing = true;

                if (!enableAutoIncrement) // Incrémente uniquement si le mode auto est désactivé
                {
                    currentClipIndex++;
                }
            }
        }

        countdownCoroutine = null;
    }

    public void StartIndexIncrementCountdown()
    {
        if (moonMovementScript == null)
        {
            Debug.LogError("MoonMovement script not assigned or incorrect type.");
            return;
        }

        tripDuration = moonMovementScript.GetDuration();
        var clips = GetCurrentAudioClips();

        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("Invalid audio clip list.");
            return;
        }

        // --- Sélection des index selon la durée ---
        scheduledIndexes.Clear();

        if (Mathf.Approximately(tripDuration, 120f))
        {
            scheduledIndexes.AddRange(new int[] { 2, 4, 6 });
        }
        else if (Mathf.Approximately(tripDuration, 180f))
        {
            scheduledIndexes.AddRange(new int[] { 0, 2, 4, 6, 8 });
        }
        else if (Mathf.Approximately(tripDuration, 240f))
        {
            scheduledIndexes.AddRange(new int[] { 0, 2, 3, 4, 5, 6, 8 });
        }
        else if (tripDuration >= 300f)
        {
            // 5 minutes ou plus: tous les clips une seule fois
            for (int i = 0; i < clips.Count; i++)
                scheduledIndexes.Add(i);
        }
        else
        {
            Debug.LogWarning($"[AutoIncrement] Unexpected duration: {tripDuration}");
        }

        // --- Réglage de l'incrément ---
        autoIncrementInterval = 30f; // Toutes les 30 secondes
        nextAutoIncrementTime = Time.time + autoIncrementInterval;
        currentClipIndex = 0;
        enableAutoIncrement = true;

        Debug.Log($"[AutoIncrement] Trip={tripDuration}s | Scheduled={scheduledIndexes.Count} clips | Every {autoIncrementInterval}s");
    }


    public void StopIndexIncrementCountdown()
    {
        //Debug.Log("StopIndexIncrementCountdown.");
        enableAutoIncrement = false;
        //currentClipIndex = 0;
    }

    void Update()
    {
        if (playing)
        {
            if (!audioSource.isPlaying)
            {
                playing = false;
                isStoppedExternally = false;
                OnFinishPlayback.Invoke();
            }
        }

        var clips = GetCurrentAudioClips();

        if (enableAutoIncrement && scheduledPointer < scheduledIndexes.Count)
        {
            // Déclenche le changement d’index 1s avant le moment exact
            while (Time.time >= nextAutoIncrementTime - 1f && scheduledPointer < scheduledIndexes.Count)
            {
                int clipIndex = scheduledIndexes[scheduledPointer];
                this.currentClipIndex = clipIndex; // autre script lira bien la bonne valeur

                Debug.Log($"[AutoIncrement]: Set currentClipIndex = {clipIndex} (scheduledPointer {scheduledPointer}) at {Time.time - Time.timeSinceLevelLoad:F1}s");

                // On avance le pointeur pour la prochaine itération
                scheduledPointer++;
                nextAutoIncrementTime += autoIncrementInterval;
            }
        }

        if (enableAutoIncrement && scheduledPointer >= scheduledIndexes.Count)
        {
            Debug.Log("[AutoIncrement] All scheduled indexes reached.");
            StopIndexIncrementCountdown();
        }
    }
    private List<AudioClip> GetCurrentAudioClips()
    {
        return LanguageManager.Instance.CurrentLang == LanguageManager.Lang.French ? audioClipsFR : audioClips;
    }

}
