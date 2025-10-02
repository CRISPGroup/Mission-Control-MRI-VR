using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CollisionEvent
{
    public Collider targetCollider;
    public UnityEvent onEnterEvent;
    public UnityEvent onStayEvent;
    public UnityEvent onExitEvent;
}

public class HandleCollisionDetectionWithEvents : MonoBehaviour
{
    [SerializeField] private List<CollisionEvent> collisionEvents = new List<CollisionEvent>();

    public bool enableCollision = false;   

    void Start()
    {
        this.enableCollision = false;
    }

    public void EnableCollisionDetection()
    {
        this.enableCollision = true;
    }

    public void DisableCollisionDetection()
    {
        this.enableCollision = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enableCollision) return;

        foreach (CollisionEvent collisionEvent in collisionEvents)
        {
            if (other == collisionEvent.targetCollider)
            {
                collisionEvent.onEnterEvent?.Invoke();
                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enableCollision) return;

        foreach (CollisionEvent collisionEvent in collisionEvents)
        {
            if (other == collisionEvent.targetCollider)
            {
                collisionEvent.onExitEvent?.Invoke();
                break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!enableCollision) return;

        foreach (CollisionEvent collisionEvent in collisionEvents)
        {
            if (other == collisionEvent.targetCollider)
            {
                collisionEvent.onStayEvent?.Invoke();
                break;
            }
        }
    }
}
