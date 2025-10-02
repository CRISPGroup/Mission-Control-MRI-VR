using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasActivator : MonoBehaviour
{
    [System.Serializable]
    public class IgnoredCanvas
    {
        public Canvas canvas; // Canvas à ignorer
        public bool ignoreChildren; // Si true, ignore tous les canvases enfants
    }

    [SerializeField] private List<IgnoredCanvas> ignoredCanvases = new List<IgnoredCanvas>(); // Liste des canvases ignorés
    private List<Canvas> allCanvases = new List<Canvas>(); // Liste des canvases trouvés
    private HashSet<Canvas> temporarilyExcluded = new HashSet<Canvas>();

    public void FindAllActiveCanvases()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        allCanvases.Clear();

        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.enabled && canvas.gameObject.activeInHierarchy && !ShouldIgnoreCanvas(canvas))
            {
                allCanvases.Add(canvas);
            }
        }
    }

    /// <summary>
    /// Vérifie si un canvas ou ses enfants doivent être ignorés.
    /// </summary>
    private bool ShouldIgnoreCanvas(Canvas canvas)
    {
        foreach (var ignored in ignoredCanvases)
        {
            if (canvas == ignored.canvas)
            {
                return true; // Ignorer ce canvas
            }

            if (ignored.ignoreChildren && IsChildOf(canvas.transform, ignored.canvas.transform))
            {
                return true; // Ignorer les enfants de ce canvas
            }
        }
        return false;
    }

    /// <summary>
    /// Vérifie si un Transform est enfant d'un autre Transform.
    /// </summary>
    private bool IsChildOf(Transform child, Transform parent)
    {
        while (child != null)
        {
            if (child == parent)
            {
                return true;
            }
            child = child.parent;
        }
        return false;
    }

    /// <summary>
    /// Désactive tous les canvases sauf ceux à ignorer.
    /// </summary>
    public void DisableAllCanvasesExceptIgnored()
    {
        foreach (Canvas canvas in allCanvases)
        {
            canvas.enabled = false;
        }
    }
    public void ExcludeCanvasTemporarily(Canvas canvas)
    {
        if (canvas != null)
            temporarilyExcluded.Add(canvas);
    }
    public void ClearTemporaryExclusions()
    {
        temporarilyExcluded.Clear();
    }

    /// <summary>
    /// Active tous les canvases trouvés.
    /// </summary>
    public void EnableAllCanvases()
    {
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas != null && !temporarilyExcluded.Contains(canvas))
            {
                canvas.enabled = true;
            }
        }
    }
}
