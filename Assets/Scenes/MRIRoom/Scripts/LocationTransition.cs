using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class LocationTransition : MonoBehaviour
{
    [SerializeField] private FadeScreen fadeScreen;
    [SerializeField] private Transform[] locations;
    private int locationIndex = 0;

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

    /* Old recenter
     *    public void Recenter(Transform dest)
    {
        Vector3 offset = head.position - origin.position;
        offset.y = 0;

        Transform target = dest;

        origin.position = target.position - offset;

        float heightAdjustment = target.position.y - head.position.y;
        origin.position += new Vector3(0, heightAdjustment, 0);

        Vector3 targetForward = target.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        Vector3 cameraForward = head.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Correction for important tilts
        float tiltAngle = Vector3.Angle(Vector3.up, head.up);

        if (tiltAngle > 80f)
        {
            Vector3 directionToOrigin = (origin.position - head.position).normalized;
            directionToOrigin.y = 0;

            cameraForward = Vector3.ProjectOnPlane(directionToOrigin, Vector3.up).normalized;

        }

        cameraForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up).normalized;

        float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);

        origin.RotateAround(head.position, Vector3.up, angle);
    }

    /*
     * Recentering player so that they are aligned with a target
     * 
     * Update: Made this work even if the player tilts backwards past 90 degres
     * I first detected the head tilt by measuring the angle between the normal vertical direction (Vector3.up) and the orientation of the head (head.up). 
     * When the player is tilted more than 80 degrees, I calculate a direction vector from the center of the XR Origin to the head and then "flatten"
     * it to keep it level with the ground (project it onto the horizontal plane). 
     * This helps the system determine which way the player is facing and ensures proper alignment before recentering..
     * 
     * Update:
     * Changed everything, now the world rotates to be in front of the camera
     * 
     */

    /*
        public void Recenter(Transform dest)
        {
            // Calculer le décalage entre le joueur et l'origine
            Vector3 offset = head.position - origin.position;
            offset.y = 0;

            // Appliquer le décalage pour que le joueur soit aligné avec le point cible
            Vector3 newOriginPosition = dest.position - offset;
            newOriginPosition.y = origin.position.y; // Garder la hauteur initiale de l'origine
            float heightAdjustment = dest.position.y - head.position.y;
            newOriginPosition.y += heightAdjustment;

            // Ajuster la rotation pour aligner le joueur avec le target
            Vector3 targetForward = dest.forward;
            targetForward.y = 0;
            targetForward.Normalize();

            Vector3 cameraForward = head.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            // Calculer l'angle de rotation nécessaire
            float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);

            // Réaligner l'environnement complet
            Transform environment = origin; // Assumer que "origin" est le parent de tout l'environnement
            environment.position = newOriginPosition;
            environment.RotateAround(head.position, Vector3.up, angle);

            // Gestion spécifique pour les inclinaisons > 80 degrés
            float tiltAngle = Vector3.Angle(Vector3.up, head.up);
            if (tiltAngle > 80f)
            {
                // Réinitialiser complètement la position et l'orientation de l'environnement
                environment.position = -head.position;
                environment.Rotate(Vector3.up, -angle);

                // Alignement final avec le target
                Vector3 directionToTarget = dest.position - head.position;
                //directionToTarget.y = 0;
                environment.position += directionToTarget;
            }

        }
    */

    //See deepseek for the other recentering method
    /*
    public void Recenter(Transform dest)
    {
        // Calculer le décalage entre le joueur et l'origine
        Vector3 offset = head.position - origin.position;
        offset.y = 0;

        // Appliquer le décalage pour que le joueur soit aligné avec le point cible
        Vector3 newOriginPosition = dest.position - offset;
        newOriginPosition.y = origin.position.y; // Garder la hauteur initiale de l'origine
        float heightAdjustment = dest.position.y - head.position.y;
        newOriginPosition.y += heightAdjustment;

        // Ajuster la rotation pour aligner le joueur avec le target
        Vector3 targetForward = dest.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        Vector3 cameraForward = head.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Calculer l'angle de rotation nécessaire
        float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);

        // Réaligner l'environnement complet
        Transform environment = origin; // Assumer que "origin" est le parent de tout l'environnement
        environment.position = newOriginPosition;
        environment.RotateAround(head.position, Vector3.up, angle);

        // Gestion spécifique pour les inclinaisons > 80 degrés
        float tiltAngle = Vector3.Angle(Vector3.up, head.up);
        if (tiltAngle > 80f)
        {
            // Réinitialiser complètement la position et l'orientation de l'environnement
            environment.position = -head.position;
            environment.Rotate(Vector3.up, -angle);

            // Alignement final avec le target
            Vector3 directionToTarget = dest.position - head.position;
            //directionToTarget.y = 0;
            environment.position += directionToTarget;
        }

        SaveReferenceOrientation();

    }
    */
    /*
    public void Recenter(Transform target)
    {
        //Debug.Log("Recenter called with target: " + target.name);
        Vector3 offset = head.position - origin.position;
        offset.y = 0;
        origin.position = target.position - offset;
        float heightAdjustment = target.position.y - head.position.y;
        origin.position += new Vector3(0, heightAdjustment, 0);
        Vector3 targetForward = target.forward;
        targetForward.y = 0;
        targetForward.Normalize();
        Vector3 cameraForward = head.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);
        origin.RotateAround(head.position, Vector3.up, angle);
        SaveReferenceOrientation();
    }
    */
    /*
    public void Recenter(Transform target)
    {
        // Compute vector between XR origin and head
        Vector3 headToOrigin = origin.position - head.position;

        // Move origin so that the head is at the target position
        Vector3 newOriginPosition = target.position + headToOrigin;
        origin.position = newOriginPosition;

        Vector3 playerForward = head.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        Vector3 targetForward = target.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        // Étape 3 : calcule l’angle entre les deux
        float angle = Vector3.SignedAngle(playerForward, targetForward, Vector3.up);

        origin.RotateAround(head.position, Vector3.up, angle);
        SaveReferenceOrientation();

        /* Étape 5 : positionne le rig pour que la tête se retrouve au bon endroit
        Vector3 headToRigOffset = head.position - origin.position;
        Vector3 newOriginPosition = newLocation.position - headToRigOffset;
        origin.position = newOriginPosition; 
    }
    */
    public void Recenter(Transform target)
    {
        // 1. Calcul du décalage local (position + rotation de la tête dans le XR Origin)
        Matrix4x4 originToWorld = origin.localToWorldMatrix;
        Matrix4x4 worldToOrigin = originToWorld.inverse;

        Vector3 headLocalPos = worldToOrigin.MultiplyPoint(head.position);
        Quaternion headLocalRot = Quaternion.Inverse(origin.rotation) * head.rotation;

        // 2. Cible à atteindre
        Vector3 desiredHeadWorldPos = target.position;
        Quaternion desiredHeadWorldRot = target.rotation;

        // 3. Calculer la rotation globale du XR Origin
        Quaternion newOriginRot = desiredHeadWorldRot * Quaternion.Inverse(headLocalRot);

        // Correction : on garde seulement la composante Y de cette rotation
        Vector3 euler = newOriginRot.eulerAngles;
        newOriginRot = Quaternion.Euler(0, euler.y, 0);

        // 4. Recalculer la position avec la rotation nettoyée
        Vector3 newOriginPos = desiredHeadWorldPos - newOriginRot * headLocalPos;

        // 5. Appliquer la nouvelle pose à l'Origin
        origin.SetPositionAndRotation(newOriginPos, newOriginRot);

        SaveReferenceOrientation();
    }

    /*
    public void Recenter(Transform target)
    {
        Matrix4x4 originToWorld = origin.localToWorldMatrix;
        Matrix4x4 worldToOrigin = originToWorld.inverse;

        Vector3 headLocalPos = worldToOrigin.MultiplyPoint(head.position);
        Quaternion headLocalRot = Quaternion.Inverse(origin.rotation) * head.rotation;

        // 2. On veut que cette tête locale soit maintenant placée à la position/rotation du target
        // => calculer l'origine idéale pour ça

        // Cible = où on veut que la tête soit
        Vector3 desiredHeadWorldPos = target.position;
        Quaternion desiredHeadWorldRot = target.rotation;

        // Nouvelle origin = on remonte de headLocal vers le monde
        Quaternion newOriginRot = desiredHeadWorldRot * Quaternion.Inverse(headLocalRot);
        Vector3 newOriginPos = desiredHeadWorldPos - newOriginRot * headLocalPos;

        // 3. Appliquer
        origin.SetPositionAndRotation(newOriginPos, newOriginRot);
    }
    */

    public void Recenter()
    {
        Transform target = locations[locationIndex % locations.Length];
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
        locationIndex = index;
    }

    public void GoToSpecificLocation(int index)
    {
        locationIndex = index;
        StartCoroutine(GoToLocation(locations[locationIndex % locations.Length]));
    }

    public void GoToSpecificLocation(Transform target)
    {
        StartCoroutine(GoToLocation(target));
    }

    public void GoToNextLocation()
    {
        locationIndex++;
        //Debug.Log("GoToNextLocation called");
        StartCoroutine(GoToLocation(locations[locationIndex % locations.Length]));
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


    public IEnumerator GoToLocation(Transform newLocation)
    {
        fadeScreen.SetFadeDuration(locationTransitionDuration);
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(locationTransitionDuration);

        // Étape 1 : direction neutre du joueur dans le monde actuel
        Vector3 currentWorldNeutralDirection = origin.rotation * Quaternion.Inverse(savedOriginRotation) * savedHeadForward;
        currentWorldNeutralDirection.y = 0;
        currentWorldNeutralDirection.Normalize();

        // Étape 2 : direction réelle du `dest` (où il regarde)
        Vector3 destinationForward = newLocation.forward;
        destinationForward.y = 0;
        destinationForward.Normalize();

        // Étape 3 : calcule l’angle entre les deux
        float angle = Vector3.SignedAngle(currentWorldNeutralDirection, destinationForward, Vector3.up);

        // Étape 4 : applique la rotation autour de la tête
        origin.RotateAround(head.position, Vector3.up, angle);

        // Étape 5 : positionne le rig pour que la tête se retrouve au bon endroit
        Vector3 headToRigOffset = head.position - origin.position;
        Vector3 newOriginPosition = newLocation.position - headToRigOffset;
        origin.position = newOriginPosition;

        fadeScreen.FadeIn();
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

    /*public void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "OnMoon")
        {
            GoToSpecificLocationByName("Anchor_welcome");
        }
    }
    */

}