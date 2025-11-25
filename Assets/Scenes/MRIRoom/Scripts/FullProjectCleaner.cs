#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;

public static class FullProjectCleaner
{
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
