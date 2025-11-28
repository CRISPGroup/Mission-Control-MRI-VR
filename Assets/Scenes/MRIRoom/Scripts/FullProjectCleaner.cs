#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Provides a set of editor tools for safely cleaning a Unity project.
/// Includes utilities for:
/// - Removing missing MonoBehaviour scripts
/// - Detecting renderers without materials
/// - Clearing project cache folders (Library, Temp, obj)
/// - Clearing editor-specific caches (Inspector, ScriptAssemblies)
/// - Resetting lightmap-related material keywords
///
/// These tools are accessible via the Unity Editor menu under:
/// <b>Tools > Clean</b>
/// </summary>
/// <remarks>
/// <para><b>Safety Note:</b> The cleaning actions here modify project assets and files.
/// Always commit or back up your project before running the cache deletion functions.</para>
/// <para>This script is editor-only and excluded from runtime builds.</para>
/// </remarks>
public static class FullProjectCleaner
{
    /// <summary>
    /// Scans all scene GameObjects for missing MonoBehaviours and
    /// renderers without assigned materials.  
    /// Removes missing scripts safely and logs detected issues.
    /// </summary>
    [MenuItem("Tools/Clean/Clean Missing Scripts & Renderers (Safe)")]
    static void CleanMissingAndBrokenRenderers()
    {
        int removedScripts = 0;
        int missingMats = 0;
        int checkedObjects = 0;

        // Deselect everything to avoid inspector serialization errors
        Selection.objects = new Object[0];

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null)
                continue;

            checkedObjects++;
            Undo.RegisterCompleteObjectUndo(go, "Clean Missing Scripts");

            // Remove missing MonoBehaviours safely
            removedScripts += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            // Check for renderers without materials
            var renderer = go.GetComponent<Renderer>();
            if (renderer && renderer.sharedMaterial == null)
            {
                Debug.LogWarning("Renderer without material: " + go.name, go);
                missingMats++;
            }
        }

        // Mark all scenes as dirty so Unity knows changes occurred
        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Clean Complete",
            "Checked " + checkedObjects + " objects.\n" +
            "Removed " + removedScripts + " missing scripts.\n" +
            "Found " + missingMats + " renderers without materials.", "OK");

        Debug.Log("Clean done: " + removedScripts + " missing scripts removed, " + missingMats + " missing materials detected.");
    }

    /// <summary>
    /// Deletes the <b>Library/</b>, <b>Temp/</b>, and <b>obj/</b> folders in the project.
    /// Unity will automatically rebuild them the next time it opens.  
    /// This can help fix serialization or import cache issues.
    /// </summary>
    [MenuItem("Tools/Clean/Clear Unity Cache (Library, Temp, obj)")]
    static void ClearUnityCache()
    {
        if (!EditorUtility.DisplayDialog("Clear Unity Cache?",
            "This will delete Library/, Temp/, and obj/ folders.\nUnity will rebuild them on restart.\n\nProceed?",
            "Yes, clear", "Cancel"))
            return;

        string projectPath = Application.dataPath.Replace("/Assets", "");
        string[] targets = { "Library", "Temp", "obj" };

        foreach (var folder in targets)
        {
            string fullPath = Path.Combine(projectPath, folder);
            if (Directory.Exists(fullPath))
            {
                try
                {
                    Directory.Delete(fullPath, true);
                    Debug.Log("Deleted " + folder + "/");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Couldn't delete " + folder + ": " + ex.Message);
                }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", "Cache cleared. Please restart Unity.", "OK");
    }

    /// <summary>
    /// Clears cached editor data such as <b>ScriptAssemblies</b> and Inspector settings.
    /// This can resolve persistent editor errors without affecting project assets.
    /// </summary>
    [MenuItem("Tools/Clean/Clear Editor Cache (Inspector, ScriptAssemblies)")]
    static void ClearEditorCache()
    {
        // Clear editor selection and caches
        Selection.objects = new Object[0];
        Resources.UnloadUnusedAssets();
        Caching.ClearCache();
        AssetDatabase.Refresh();

        string projectPath = Application.dataPath.Replace("/Assets", "");
        string[] cacheDirs = {
            "Library/ScriptAssemblies",
            "Library/InspectorExpandedItems.asset"
        };

        foreach (var path in cacheDirs)
        {
            string full = Path.Combine(projectPath, path);
            if (File.Exists(full))
            {
                try
                {
                    File.Delete(full);
                    Debug.Log("Deleted " + path);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Could not delete " + path + ": " + e.Message);
                }
            }
        }

        EditorUtility.DisplayDialog("Cache cleared", "Editor cache cleared.\nPlease restart Unity manually now.", "OK");
    }

    /// <summary>
    /// Scans all materials in the project and disables obsolete lightmap keywords
    /// such as <c>LIGHTMAP_ON</c> and <c>DIRLIGHTMAP_COMBINED</c>.  
    /// Helps reduce shader variant clutter in builds.
    /// </summary>
    [MenuItem("Tools/Cleanup Lightmap Keywords")]
    public static void Cleanup()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.IsKeywordEnabled("LIGHTMAP_ON") || mat.IsKeywordEnabled("DIRLIGHTMAP_COMBINED"))
            {
                mat.DisableKeyword("LIGHTMAP_ON");
                mat.DisableKeyword("DIRLIGHTMAP_COMBINED");
                Debug.Log($"[Cleanup] Fixed keywords on {mat.name}");
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
