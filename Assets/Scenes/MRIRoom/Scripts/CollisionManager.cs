using UnityEngine;

/// <summary>
/// Handles trigger-based collision detection for a group of predefined colliders,
/// delegating scanner-related collision events to a <see cref="LocationTransition"/> script.
/// </summary>
/// <remarks>
/// TODO: Refactor so the parent object manages collisions for all child colliders automatically.
/// </remarks>

public class CollisionManager : MonoBehaviour
{
    [Header("Colliders to monitor")]
    [Tooltip("Collider representing the top area of interaction.")]
    [SerializeField] private GameObject collider1;

    [Tooltip("Collider representing the bottom area of interaction.")]
    [SerializeField] private GameObject collider2;

    [Tooltip("Collider representing the exit area of interaction.")]
    [SerializeField] private GameObject collider3;

    [Header("Linked Scripts")]
    [Tooltip("Reference to the LocationTransition script handling scanner logic.")]
    [SerializeField] private LocationTransition locaScript;


    /// <summary>
    /// Called while another collider stays within this trigger.
    /// Detects specific colliders and triggers corresponding scanner events.
    /// </summary>
    /// <param name="other">The collider currently intersecting this trigger.</param>
    public void OnTriggerStay(Collider other)
    {
        //Debug.Log("Collision with: " + other.gameObject);
        if (other.gameObject == collider1)
        {
            HandlePlayerTopCollision();
        }

        if (other.gameObject == collider2)
        {
            HandlePlayerBottomCollision();
        }
    }

    /// <summary>
    /// Called when another collider exits this trigger.
    /// Triggers exit handling if the collider matches the configured exit object.
    /// </summary>
    /// <param name="other">The collider that exited the trigger area.</param>
    public void OnTriggerExit(Collider other)
    {

        if (other.gameObject == collider3)
        {
            HandlePlayerExitingArea();
        }
    }

    /// <summary>
    /// Handles logic when the player exits the defined interaction area.
    /// (e.g., fade-out, show UI hints, manage re-entry logic.)
    /// </summary>
    public void HandlePlayerExitingArea()
    {
        //Fade out (almost fully dark)
        //Display outside the zone canvas
        //arrow + highligh zone
        //handle reintering area (fade in / hide canvas..)
    }

    /// <summary>
    /// Handles collision logic when the player interacts with the top scanner zone.
    /// </summary>
    public void HandlePlayerTopCollision()
    {
        //Debug.Log("Player has entered the trigger area.");
        locaScript.HandleScannerTopCollision();
    }

    /// <summary>
    /// Handles collision logic when the player interacts with the bottom scanner zone.
    /// </summary>
    public void HandlePlayerBottomCollision()
    {
        //Debug.Log("Player has entered the trigger area.");
        locaScript.HandleScannerBottomCollision();
    }
}
