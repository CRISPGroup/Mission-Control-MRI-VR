using UnityEngine;
using System.Collections;

public class DragXRRig : MonoBehaviour
{
    [SerializeField] private Transform xRRigTransform;
    [SerializeField] private float dragSpeed = 1.0f;
    [SerializeField] private Transform bedInitialPosition;

    private Vector3 previousPosition;
    private bool isDragging = false;

    void Start()
    {
        transform.position = bedInitialPosition.position;
        previousPosition = transform.position;
    }

    void Update()
    {
        if (!isDragging && transform.position != previousPosition)
        {
            PerformDragging();
        }
    }

    public void PerformDragging()
    {
        //Debug.Log("Perform Dragging..");
        previousPosition = transform.position;
        isDragging = true;
        StartCoroutine(DoDragging());
    }

    IEnumerator DoDragging()
    {
        //Debug.Log("Do Dragging..");
        while (isDragging)
        {
            Vector3 currentPosition = transform.position;
            Vector3 movement = currentPosition - previousPosition;
            xRRigTransform.position += movement;
            //Debug.Log("XRRig pos: " + xRRigTransform.position);

            previousPosition = currentPosition;

            yield return null;
        }

        isDragging = false;
    }
}
