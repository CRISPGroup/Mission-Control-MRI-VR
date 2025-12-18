using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defines a single collision rule, pairing a specific collider with UnityEvents
/// for OnTriggerEnter, OnTriggerStay, and OnTriggerExit.
/// </summary>
[System.Serializable]
public class CollisionEvent
{
    [Tooltip("The specific collider that will trigger this event set.")]
    public Collider targetCollider;

    [Tooltip("Event invoked when the collider first enters the trigger area.")]
    public UnityEvent onEnterEvent;

    [Tooltip("Event invoked while the collider remains inside the trigger area.")]
    public UnityEvent onStayEvent;

    [Tooltip("Event invoked when the collider exits the trigger area.")]
    public UnityEvent onExitEvent;
}

/// <summary>
/// Handles collision detection for a configurable list of target colliders,
/// triggering custom UnityEvents when they enter, stay, or exit the trigger zone.
/// </summary>
/// <remarks>
/// - Collision detection can be globally toggled on or off at runtime.  
/// - Each collider can have independent events for enter, stay, and exit.  
/// - Designed for flexible gameplay triggers (e.g., proximity effects, UI zones, VR interactions).
/// </remarks>
public class HandleCollisionDetectionWithEvents : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("List of specific colliders and their associated UnityEvents.")]
    [SerializeField] private List<CollisionEvent> collisionEvents = new List<CollisionEvent>();

    [Tooltip("Global toggle for collision detection.")]
    public bool enableCollision = false;

    /// <summary>
    /// Initializes the component with collision detection disabled by default.
    /// </summary>
    void Start()
    {
        this.enableCollision = false;
    }

    /// <summary>
    /// Enables all collision event detection.
    /// </summary>
    public void EnableCollisionDetection()
    {
        this.enableCollision = true;
    }

    /// <summary>
    /// Disables all collision event detection.
    /// </summary>
    public void DisableCollisionDetection()
    {
        this.enableCollision = false;
    }

    /// <summary>
    /// Invokes the corresponding OnEnter event when a tracked collider enters the trigger.
    /// </summary>
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

    /// <summary>
    /// Invokes the corresponding OnExit event when a tracked collider leaves the trigger.
    /// </summary>
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

    /// <summary>
    /// Continuously invokes the OnStay event while a tracked collider remains inside the trigger.
    /// </summary>
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
