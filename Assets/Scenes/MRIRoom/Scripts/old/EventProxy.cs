using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventProxy : MonoBehaviour
{
    [SerializeField] bool TriggerOnStart = false;
    [SerializeField] int TriggerOnStartNum = 0;
    [SerializeField] bool Once;
    [SerializeField] List<UnityEvent> Events;
    [SerializeField] float Delay;

    bool played;
    [ContextMenu("Trigger")]
    public void TriggerEvent(int i)
    {
        if (Once && played) return;
        if (Once) played = true;
        if (Delay == 0 && !paused)
        {
            Events[i]?.Invoke();
        }
        else
            StartCoroutine(DelayedEvent(i, Delay));
    }
    IEnumerator DelayedEvent(int i, float time)
    {
        yield return new WaitForSeconds(time);
        while (paused) yield return null;
        Events[i]?.Invoke();
    }
    public void SilentTrigger()
    {
        played = true;
    }

    void Start()
    {
        if (TriggerOnStart && Delay == 0) TriggerEvent(TriggerOnStartNum);
        if (TriggerOnStart && Delay != 0) StartCoroutine(DelayedEvent(TriggerOnStartNum, Delay));
    }

    bool paused;
    void Pause(bool pause)
    {
        paused = pause;
    }

}