using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prewarms critical materials by rendering a tiny invisible quad with each of them.
/// This forces Unity to compile the GPU programs (CreateGPUProgram) and link pipelines
/// before gameplay, avoiding spikes or stutters during teleportation or heavy scenes.
/// </summary>
public class MaterialPrewarm : MonoBehaviour
{
    [Tooltip("List all the materials you want to prewarm (URP/Lit, URP/Unlit, custom shaders, etc.)")]
    [SerializeField] private Material[] materialsToPrewarm;

    [Tooltip("Number of materials to warm up per frame to avoid blocking the main thread.")]
    [SerializeField] private int materialsPerFrame = 2;

    private Mesh quadMesh;
    private Camera hiddenCamera;

    private void Awake()
    {
        // Create a hidden camera for offscreen rendering
        GameObject camObj = new GameObject("MaterialPrewarmCamera");
        hiddenCamera = camObj.AddComponent<Camera>();
        hiddenCamera.enabled = false;
        hiddenCamera.clearFlags = CameraClearFlags.Nothing;
        hiddenCamera.cullingMask = 0;
    }

    private void Start()
    {
        StartCoroutine(WarmupMaterialsGradually());
    }

    private IEnumerator WarmupMaterialsGradually()
    {
        //Debug.Log($"Starting material prewarm for {materialsToPrewarm.Length} materials...");
        quadMesh = GenerateQuad();

        // Use a tiny offscreen RenderTexture so nothing is visible to the user
        RenderTexture rt = new RenderTexture(4, 4, 0);
        RenderTexture.active = rt;

        int count = 0;
        foreach (Material mat in materialsToPrewarm)
        {
            if (mat == null) continue;

            // Draw a simple quad with the material to force GPU pipeline creation
            Graphics.DrawMesh(quadMesh, Matrix4x4.identity, mat, 0);
            count++;

            // Spread the work over multiple frames to avoid stutters
            if (count % materialsPerFrame == 0)
                yield return null;
        }

        RenderTexture.active = null;
        rt.Release();
        //Debug.Log("Material prewarming complete.");
    }

    private Mesh GenerateQuad()
    {
        Mesh m = new Mesh();
        m.vertices = new Vector3[]
        {
            new Vector3(-1, -1, 0),
            new Vector3( 1, -1, 0),
            new Vector3( 1,  1, 0),
            new Vector3(-1,  1, 0)
        };
        m.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        m.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        return m;
    }
}
