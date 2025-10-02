using UnityEngine;

public class ScenePersistor : MonoBehaviour
{
    public static ScenePersistor instance;

    private void Awake()
    {
        // Assurer qu'il n'y ait qu'un seul GameManager et le garder entre les scènes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Garde ce GameObject entre les scènes
        }
        else
        {
            Destroy(gameObject); // Détruit ce GameObject s'il existe déjà
        }
    }
}
