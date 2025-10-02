using UnityEngine;
using UnityEngine.Events;

public class ConfirmLookDirection : MonoBehaviour
{
    [SerializeField] private Transform vrCamera; // Caméra du joueur (VR ou non)
    [SerializeField] private Vector3 targetDirection = Vector3.up; // Direction cible (par défaut : vers le haut)
    [SerializeField] private float angleThreshold = 15f; // Angle d'acceptation en degrés
    [SerializeField] private UnityEvent onLookConfirmed; // Événement déclenché si l'utilisateur regarde dans la bonne direction
    [SerializeField] private UnityEvent onLookUnconfirmed;

    void Start()
    {
        if (vrCamera == null)
        {
            Debug.LogError("VR camera is not assigned!");
        }
    }

    void Update()
    {
        if (vrCamera == null) return;

        float angle = Vector3.Angle(vrCamera.forward, targetDirection);

        if (angle <= angleThreshold)
        {
            onLookConfirmed?.Invoke();
        }
        else
        {
            onLookUnconfirmed?.Invoke();
        }
    }

    /// <summary>
    /// Met à jour dynamiquement la direction cible et l'angle d'acceptation.
    /// </summary>
    /// <param name="newDirection">Nouvelle direction cible</param>
    /// <param name="newThreshold">Nouveau seuil d'angle</param>
    public void SetTargetDirection(Vector3 newDirection, float newThreshold = 15f)
    {
        targetDirection = newDirection;
        angleThreshold = newThreshold;
    }
}
