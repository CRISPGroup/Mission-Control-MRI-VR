using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocationSetup
{
    public Transform locationPoint;
    public List<GameObject> objectsToEnable;
    public List<GameObject> objectsToDisable;
}
public class LocationTransition : MonoBehaviour
{
    [SerializeField] private FadeScreen fadeScreen;
    [SerializeField] private List<LocationSetup> locations;
    private int locationIndex = 0;
    private LocationSetup previousLocationSetup;

    [SerializeField] private Transform head;
    [SerializeField] private Transform origin;
    [SerializeField] private float locationTransitionDuration = 2f;

    private Quaternion savedOriginRotation;
    private Vector3 savedHeadForward;

    public void Start()
    {
        //StartCoroutine(GoToFirstLocation(1));
        SaveReferenceOrientation();
    }

    public void SetLocationTransitionDuration(float duration)
    {
        locationTransitionDuration = duration;
    }   

    public void SaveReferenceOrientation()
    {
        savedOriginRotation = origin.rotation;

        Vector3 forward = head.forward;
        forward.y = 0;
        savedHeadForward = forward.normalized;
    }


    public void HandleScannerTopCollision()
    {
        Transform newTransform = origin;
        Vector3 newPosition = newTransform.position;
        newPosition.y = -.35f;
        origin.position = newPosition;
        newTransform.position = newPosition;
        //Recenter(newTransform);
    }

    public void HandleScannerBottomCollision()
    {
        Transform newTransform = origin;
        Vector3 newPosition = newTransform.position;
        newPosition.y = 0.175f;
        origin.position = newPosition;
        newTransform.position = newPosition;
        //Recenter(newTransform);
    }
    public void Recenter(Transform target)
    {
        // Calcul du décalage local (position + rotation de la tête dans le XR Origin)
        Matrix4x4 originToWorld = origin.localToWorldMatrix;
        Matrix4x4 worldToOrigin = originToWorld.inverse;

        Vector3 headLocalPos = worldToOrigin.MultiplyPoint(head.position);
        Quaternion headLocalRot = Quaternion.Inverse(origin.rotation) * head.rotation;

        // Cible à atteindre
        Vector3 desiredHeadWorldPos = target.position;
        Quaternion desiredHeadWorldRot = target.rotation;

        // Calculer la rotation globale du XR Origin
        Quaternion newOriginRot = desiredHeadWorldRot * Quaternion.Inverse(headLocalRot);

        // Correction : on garde seulement la composante Y de cette rotation
        Vector3 euler = newOriginRot.eulerAngles;
        newOriginRot = Quaternion.Euler(0, euler.y, 0);

        // Recalculer la position avec la rotation nettoyée
        Vector3 newOriginPos = desiredHeadWorldPos - newOriginRot * headLocalPos;

        // Appliquer la nouvelle pose à l'Origin
        origin.SetPositionAndRotation(newOriginPos, newOriginRot);

        SaveReferenceOrientation();
    }

    public void Recenter()
    {
        Transform target = locations[locationIndex % locations.Count].locationPoint;
        Recenter(target);
    }

    public IEnumerator GoToFirstLocation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Recenter();
    }

    public void IncrementLocation()
    {
        locationIndex++;
    }

    public void SetLocationIndex(int index)
    {
        if (index < 0 || index >= locations.Count)
        {
            Debug.LogWarning($"Invalid location index: {index}");
            return;
        }

        previousLocationSetup = locations[locationIndex];

        locationIndex = index;
    }

    public void GoToSpecificLocation(int index)
    {
        if (index < 0 || index >= locations.Count)
        {
            Debug.LogWarning($"Invalid location index: {index}");
            return;
        }
        previousLocationSetup = locations[locationIndex];
        locationIndex = index;

        StartCoroutine(GoToLocation(locations[locationIndex]));
    }

    public void GoToSpecificLocation(Transform target)
    {
        LocationSetup setup = locations.Find(l => l.locationPoint == target);
        if (setup == null)
        {
            Debug.LogWarning($"No LocationSetup found for transform {target.name}");
            return;
        }

        previousLocationSetup = locations[locationIndex];
        locationIndex = locations.IndexOf(setup);

        StartCoroutine(GoToLocation(setup));
    }

    public void GoToNextLocation()
    {
        previousLocationSetup = locations[locationIndex];
        locationIndex = (locationIndex + 1) % locations.Count;

        StartCoroutine(GoToLocation(locations[locationIndex]));
    }

    public void ResetCurrentLocation()
    {
        Recenter();
    }
    public void EndLocation()
    {
        //StartCoroutine(PerformEndLocation(1f));
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
    }
    public IEnumerator GoToLocation(LocationSetup newSetup)
    {
        Transform newLocation = newSetup.locationPoint;

        fadeScreen.SetFadeDuration(locationTransitionDuration);
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(locationTransitionDuration);

        // (Optionnel) Attendre une petite marge (0.2 à 1s) pour s’assurer du noir total
        yield return new WaitForSeconds(0.5f);

        if (previousLocationSetup != null)
        {
            foreach (var go in previousLocationSetup.objectsToDisable)
                if (go) go.SetActive(false);
        }

        foreach (var go in newSetup.objectsToEnable)
            if (go) go.SetActive(true);

        // direction neutre du joueur dans le monde actuel
        Vector3 currentWorldNeutralDirection = origin.rotation * Quaternion.Inverse(savedOriginRotation) * savedHeadForward;
        currentWorldNeutralDirection.y = 0;
        currentWorldNeutralDirection.Normalize();

        // direction réelle du dest (où il regarde)
        Vector3 destinationForward = newLocation.forward;
        destinationForward.y = 0;
        destinationForward.Normalize();

        // calculer langle entre les deux
        float angle = Vector3.SignedAngle(currentWorldNeutralDirection, destinationForward, Vector3.up);

        // appliquer la rotation autour de la tête
        origin.RotateAround(head.position, Vector3.up, angle);

        // positionner le rig pour que la tête se retrouve au bon endroit
        Vector3 headToRigOffset = head.position - origin.position;
        Vector3 newOriginPosition = newLocation.position - headToRigOffset;
        origin.position = newOriginPosition;

        // Attendre encore un court instant si besoin avant le fade-in
        yield return new WaitForSeconds(0.2f);

        fadeScreen.FadeIn();

        previousLocationSetup = newSetup;
    }

    public IEnumerator PerformEndLocation(float fadeDuration)
    {
        fadeScreen.SetFadeDuration(locationTransitionDuration);
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeDuration - 0.5f);
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                        Application.Quit();
        #endif
    }
}