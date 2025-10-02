using UnityEngine;

public class ScenePersistor : MonoBehaviour
{
    public static ScenePersistor instance;

    private void Awake()
    {
        // Assurer qu'il n'y ait qu'un seul GameManager et le garder entre les sc�nes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Garde ce GameObject entre les sc�nes
        }
        else
        {
            Destroy(gameObject); // D�truit ce GameObject s'il existe d�j�
        }
    }
}
