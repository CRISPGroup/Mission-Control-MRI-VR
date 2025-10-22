using System.Collections.Generic;
using UnityEngine;

public class CanvasActivator : MonoBehaviour
{
    [System.Serializable]
    public class IgnoredCanvas
    {
        public Canvas canvas;
        public bool ignoreChildren;
    }

    [SerializeField] private List<IgnoredCanvas> ignoredCanvases = new();
    private List<Canvas> allCanvases = new();
    private Dictionary<Canvas, bool> savedCanvasStates = new();

    // -------------------- FIND --------------------

    /// <summary>
    /// Trouve tous les canvases actifs dans la scène (enabled + actifs dans la hiérarchie)
    /// </summary>
    public void FindAllActiveCanvases()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        allCanvases.Clear();

        foreach (Canvas canvas in canvases)
        {
            // On ne garde que les canvases *actuellement actifs et visibles*
            if (canvas != null && canvas.enabled && canvas.gameObject.activeInHierarchy && !ShouldIgnoreCanvas(canvas))
            {
                allCanvases.Add(canvas);
            }
        }

        Debug.Log($"[CanvasActivator] Found {allCanvases.Count} active canvases.");
    }

    private bool ShouldIgnoreCanvas(Canvas canvas)
    {
        foreach (var ignored in ignoredCanvases)
        {
            if (canvas == ignored.canvas)
                return true;
            if (ignored.ignoreChildren && IsChildOf(canvas.transform, ignored.canvas.transform))
                return true;
        }
        return false;
    }

    private bool IsChildOf(Transform child, Transform parent)
    {
        while (child != null)
        {
            if (child == parent)
                return true;
            child = child.parent;
        }
        return false;
    }

    // -------------------- DISABLE / RESTORE --------------------

    /// <summary>
    /// Désactive seulement les canvases actuellement actifs, tout en sauvegardant leur état.
    /// </summary>
    public void DisableAllCanvasesExceptIgnored()
    {
        savedCanvasStates.Clear();

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null) continue;

            savedCanvasStates[canvas] = canvas.enabled; // sauvegarde état
            if (canvas.enabled)
                canvas.enabled = false;
        }

        Debug.Log($"[CanvasActivator] Disabled {savedCanvasStates.Count} canvases (states saved).");
    }

    /// <summary>
    /// Réactive uniquement les canvases qui étaient actifs avant désactivation.
    /// </summary>
    public void RestoreCanvasStates()
    {
        foreach (var pair in savedCanvasStates)
        {
            Canvas canvas = pair.Key;
            bool wasEnabled = pair.Value;

            if (canvas != null && wasEnabled)
                canvas.enabled = true;
        }

        Debug.Log("[CanvasActivator] Restored previous canvas states.");
    }


}
