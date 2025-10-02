using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class HandleFadingCollision : MonoBehaviour
{
    [Header("Collision Events")]
    public UnityEvent onEnterEvent;
    public UnityEvent onExitEvent;
    public UnityEvent onStayEvent;

    [SerializeField]
    private string targetTag = "Blocking";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            onEnterEvent?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            onExitEvent?.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            onStayEvent?.Invoke();
        }
    }
}