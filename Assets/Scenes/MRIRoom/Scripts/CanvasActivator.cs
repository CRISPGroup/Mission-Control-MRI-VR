using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages activation and deactivation of all canvases in the current scene except specified ones,
/// with support for ignored canvases and state restoration.
/// Useful for temporarily disabling all active UI elements except specified ones (e.g., during a pause).
/// </summary>
public class CanvasActivator : MonoBehaviour
{
    /// <summary>
    /// Represents a canvas to ignore during activation/deactivation,
    /// optionally including its child canvases.
    /// </summary>
    [System.Serializable]
    public class IgnoredCanvas
    {
        [Tooltip("Canvas to exclude from activation/deactivation.")]
        public Canvas canvas;

        [Tooltip("If true, also ignores all child canvases of this one.")]
        public bool ignoreChildren;
    }

    [Header("Ignored Canvases")]
    [Tooltip("List of canvases that will remain unaffected by activation/deactivation.")]
    [SerializeField] private List<IgnoredCanvas> ignoredCanvases = new();

    private List<Canvas> allCanvases = new();
    private Dictionary<Canvas, bool> savedCanvasStates = new();

    // -------------------- FIND --------------------

    /// <summary>
    /// Finds all currently active canvases in the scene (enabled and active in hierarchy),
    /// excluding those specified in the ignored list.
    /// </summary>
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

        //Debug.Log($"[CanvasActivator] Found {allCanvases.Count} active canvases.");
    }

    /// <summary>
    /// Determines whether a canvas should be ignored based on the ignore list and hierarchy rules.
    /// </summary>
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

    /// <summary>
    /// Checks if a given transform is a child of another transform.
    /// </summary>
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
    /// Disables all currently active canvases except the ignored ones,
    /// saving their previous enabled state for later restoration.
    /// </summary>
    public void DisableAllCanvasesExceptIgnored()
    {
        savedCanvasStates.Clear();

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null) continue;

            savedCanvasStates[canvas] = canvas.enabled;
            if (canvas.enabled)
                canvas.enabled = false;
        }

        //Debug.Log($"[CanvasActivator] Disabled {savedCanvasStates.Count} canvases (states saved).");
    }

    /// <summary>
    /// Restores the enabled state of all canvases previously disabled by <see cref="DisableAllCanvasesExceptIgnored"/>.
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

        //Debug.Log("[CanvasActivator] Restored previous canvas states.");
    }
}
