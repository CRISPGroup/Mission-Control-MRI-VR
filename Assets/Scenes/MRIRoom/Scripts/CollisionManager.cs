using UnityEngine;

//TODO: Transform it so that the parent manages collision of all the children

public class CollisionManager : MonoBehaviour
{
    [SerializeField] private GameObject collider1;
    [SerializeField] private GameObject collider2;
    [SerializeField] private GameObject collider3;

    [SerializeField] private LocationTransition locaScript;

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

    public void OnTriggerExit(Collider other)
    {

        if (other.gameObject == collider3)
        {
            HandlePlayerExitingArea();
        }
    }

    public void HandlePlayerExitingArea()
    {
        //Fade out (almost fully dark)
        //Display outside the zone canvas
        //arrow + highligh zone
        //handle reintering area (fade in / hide canvas..)
    }

    public void HandlePlayerTopCollision()
    {
        //Debug.Log("Player has entered the trigger area.");
        locaScript.HandleScannerTopCollision();
    }

    public void HandlePlayerBottomCollision()
    {
        //Debug.Log("Player has entered the trigger area.");
        locaScript.HandleScannerBottomCollision();
    }
}
