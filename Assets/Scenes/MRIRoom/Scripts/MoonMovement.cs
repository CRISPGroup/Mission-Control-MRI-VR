using UnityEngine;
using UnityEngine.Events;

public class MoonMovement : MonoBehaviour
{
    private Vector3 startPosition;

    [SerializeField] private Transform moonStartPosition;
    [SerializeField] private Transform moonEndPosition;
    private Vector3 endPosition;

    [SerializeField] private Vector3 startScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 endScale = new Vector3(7f, 7f, 7f);

    [SerializeField] private float duration = 103f;

    [SerializeField] private float startRotationSpeed = 6f;

    [SerializeField] private GameObject targetCollider;
    //[SerializeField] private Vector3 startTargetColliderScale = new Vector3(0.01f, 0.01f, 0.01f);
    //[SerializeField] private Vector3 endTargetColliderScale = new Vector3(0.001f, 0.001f, 0.001f);
    [SerializeField] private float targetBaseVisualSize = 0.01f;

    [SerializeField] UnityEvent OnFinishPlayback;

    private float startTime;

    private bool isMoving = false;

    void Start()
    {

    }

    public void ResetPositionAndScale ()
    {
        startPosition = moonStartPosition.position;
        transform.position = startPosition;
        transform.localScale = startScale;
        //targetCollider.transform.localScale = startTargetColliderScale;
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    void Update()
    {
        if (isMoving)
        {
            float timeElapsed = Time.time - startTime;

            float t = Mathf.Clamp01(timeElapsed / duration);

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            Vector3 scale = Vector3.Lerp(startScale, endScale, t);
            transform.localScale = scale;

            float inverseScaleFactor = 1f / transform.localScale.x;

            const float fixedY = 0.0015f;

            targetCollider.transform.localScale = new Vector3(
                inverseScaleFactor * targetBaseVisualSize,
                fixedY,
                inverseScaleFactor * targetBaseVisualSize
            );

            float rotationSpeed = Mathf.Lerp(startRotationSpeed, 0f, t);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            if (timeElapsed >= duration)
            {
                OnFinish();
            }
        }
    }

    public void OnFinish()
    {
        isMoving = false;
        OnFinishPlayback.Invoke();
    }

    public void StartMovement()
    {
        startPosition = moonStartPosition.position;
        endPosition = moonEndPosition.position;

        transform.position = startPosition;
        transform.localScale = startScale;
        //targetCollider.transform.localScale = startTargetColliderScale;

        startTime = Time.time;
        isMoving = true;
    }

    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    public float GetDuration()
    {
        return duration;
    }
}
