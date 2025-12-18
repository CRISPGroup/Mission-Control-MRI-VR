using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Prewarms shaders, materials, and UI canvases to eliminate stutters
/// during the first few frames of scene rendering.
/// </summary>
/// <remarks>
/// This script forces Unity to compile and load all shader variants used in a scene
/// by briefly rendering small invisible quads using each material.  
/// It also initializes all <see cref="Canvas"/> and <see cref="TMP_InputField"/> components
/// to ensure UI meshes and layouts are built before the scene becomes interactive.
/// </remarks>
[DisallowMultipleComponent]
public class ScenePrewarmer : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("Parent object containing all renderers whose materials should be prewarmed.")]
    public GameObject parentRoot;

    [Tooltip("A small quad prefab used to display materials briefly for shader warming.")]
    public GameObject quadPreloader;


    [Header("Settings")]
    [Tooltip("Size of each invisible quad used for prewarming (in meters).")]
    public float quadSize = 0.01f;

    [Tooltip("If true, logs each material and canvas being prewarmed.")]
    public bool verbose = false;

    private List<GameObject> quads = new List<GameObject>();

    private void Awake()
    {
        
    }

    /// <summary>
    /// Forces all active and inactive canvases in the scene to build their geometry and layout data.
    /// </summary>
    /// <remarks>
    /// - Temporarily activates canvases to force <see cref="Canvas.ForceUpdateCanvases"/>.  
    /// - Refreshes <see cref="TMP_InputField"/> components to ensure their labels and meshes are ready.  
    /// - Optionally unloads unused assets afterward to clean up temporary allocations.
    /// </remarks>
    public void PrewarmAllCanvases()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var canvas in canvases)
        {
            bool wasActive = canvas.gameObject.activeSelf;

            // Temporarily enable to force layout rebuild
            canvas.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();

            // Force TMP input fields to build meshes
            var inputs = canvas.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var input in inputs)
                input.ForceLabelUpdate();

            // Restore original active state
            canvas.gameObject.SetActive(wasActive);

            if (verbose)
                Debug.Log($"[UIPrewarm] Prewarmed canvas: {canvas.name}");
        }

        Resources.UnloadUnusedAssets(); // Restore original active state
    }

    /// <summary>
    /// Coroutine that gathers all unique materials under <see cref="parentRoot"/>,
    /// renders them briefly using mini-quads, then destroys them after a short delay.
    /// </summary>
    /// <remarks>
    /// This effectively "pre-compiles" shaders and loads all required GPU states,
    /// reducing hitches and stalls when these materials are used later during gameplay.
    /// </remarks>
    private IEnumerator Start()
    {
        if (parentRoot == null)
        {
            Debug.LogWarning("[ScenePrewarmer] No parentRoot assigned. Skipping.");
            yield break;
        }

        Camera cam = Camera.main;

        Renderer[] renderers = parentRoot.GetComponentsInChildren<Renderer>(true);
        HashSet<Material> materials = new HashSet<Material>();

        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat != null)
                    materials.Add(mat);
            }
        }

        if (materials.Count == 0)
        {
            Debug.LogWarning("[ScenePrewarmer] No materials found under " + parentRoot.name);
            yield break;
        }

        foreach (Material mat in materials)
        {
            GameObject clone = Instantiate(quadPreloader, cam.transform);
            clone.transform.localPosition = quadPreloader.transform.localPosition;
            clone.transform.localRotation = quadPreloader.transform.localRotation;
            clone.transform.localScale = Vector3.one * quadSize;

            Renderer mr = clone.GetComponent<Renderer>();
            if (mr)
            {
                mr.material = mat;
                mr.material.EnableKeyword("LIGHTMAP_ON");
                mr.material.EnableKeyword("DIRLIGHTMAP_COMBINED");
            }

            quads.Add(clone);

            if (verbose)
                Debug.Log("[ScenePrewarmer] Added " + mat.name);

            yield return null; // Spread over multiple frames to avoid hitches
        }

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit != null)
        {
            Material emissionMat = new Material(urpLit);
            emissionMat.EnableKeyword("_EMISSION");
            emissionMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            emissionMat.EnableKeyword("_NORMALMAP");
            emissionMat.EnableKeyword("_OCCLUSIONMAP");
            emissionMat.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");

            Material transparentMat = new Material(urpLit);
            transparentMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            transparentMat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            transparentMat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            transparentMat.EnableKeyword("_METALLICSPECGLOSSMAP");
            transparentMat.EnableKeyword("_NORMALMAP");
            transparentMat.EnableKeyword("_OCCLUSIONMAP");

            List<Material> manualMats = new List<Material> { emissionMat, transparentMat };

            foreach (Material mat in manualMats)
            {
                GameObject clone = Instantiate(quadPreloader, cam.transform);
                clone.transform.localPosition = quadPreloader.transform.localPosition;
                clone.transform.localRotation = quadPreloader.transform.localRotation;
                clone.transform.localScale = Vector3.one * quadSize;

                Renderer mr = clone.GetComponent<Renderer>();
                if (mr)
                    mr.material = mat;

                quads.Add(clone);
                if (verbose)
                    Debug.Log($"[ScenePrewarmer] Manually prewarmed {mat.shader.name} with keywords: {string.Join(", ", mat.shaderKeywords)}");
                yield return null;
            }
        }

        Debug.Log("[ScenePrewarmer] Created " + quads.Count + " mini-quads for prewarming from " + parentRoot.name);

        PrewarmAllCanvases();

        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    /// <summary>
    /// Destroys all prewarm quads after a short delay to ensure all materials are loaded.
    /// </summary>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var q in quads)
            if (q != null) Destroy(q);
        quadPreloader.SetActive(false);

        Debug.Log("[ScenePrewarmer] Cleanup completed.");
    }
}
