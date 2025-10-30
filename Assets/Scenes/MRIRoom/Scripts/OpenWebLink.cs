using UnityEngine;

public class OpenWebLink : MonoBehaviour
{
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
