using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePrewarmer : MonoBehaviour
{
    [Tooltip("Parent object containing all renderers whose materials should be prewarmed.")]
    public GameObject parentRoot;

    public GameObject quadPreloader;

    [Tooltip("Size of the mini-quads in meters (keep very small).")]
    public float quadSize = 0.01f;

    [Tooltip("If true, print which materials are being used for prewarm.")]
    public bool verbose = false;

    private List<GameObject> quads = new List<GameObject>();

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

        Debug.Log("[ScenePrewarmer] Created " + quads.Count + " mini-quads for prewarming from " + parentRoot.name);

        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var q in quads)
            if (q != null) Destroy(q);
        quadPreloader.SetActive(false);

        Debug.Log("[ScenePrewarmer] Cleanup completed.");
    }
}
