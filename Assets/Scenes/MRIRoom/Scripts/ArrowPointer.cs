using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    public Transform target;            // Cible vers laquelle pointer
    public bool useRendererCenter = true; // Si vrai, pointe vers le centre du Renderer au lieu du pivot
    public float rotationSpeed = 5f;    // Vitesse de rotation

    void Update()
    {
        if (target == null) return;

        // 1. Calcul de la direction vers le centre
        Vector3 targetPoint = target.position;

        if (useRendererCenter)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                targetPoint = renderer.bounds.center;
            }
        }

        Vector3 direction = targetPoint - transform.position;

        // 2. Vérifie que la direction est significative
        if (direction.sqrMagnitude > 0.001f)
        {
            // 3. Calcule la rotation vers la cible
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 4. Corrige selon l’orientation de ton modèle (ici si la flèche pointe vers Y+)
            targetRotation *= Quaternion.Euler(90f, 0f, 0f);

            // 5. Rotation fluide
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Debug facultatif : affiche la ligne de visée
        Debug.DrawLine(transform.position, targetPoint, Color.green);
    }
}
