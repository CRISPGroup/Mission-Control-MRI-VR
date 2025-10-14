using System.Collections;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Extensions;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FadeScreen : MonoBehaviour
{
    public enum FadeBackend
    {
        Auto,
        RendererOnly,
        CompositionLayerOnly
    }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private bool fadeOnStart = true;

    [Header("Backend Control")]
    [Tooltip("Force a specific fade backend. Auto will pick CompositionLayer if available.")]
    [SerializeField] private FadeBackend forceBackend = FadeBackend.Auto;

    [Header("Composition Layer Setup")]
    [Tooltip("Child GameObject with CompositionLayer + ColorScaleBiasExtension.")]
    [SerializeField] private GameObject compositionLayerGO;

    [Header("Renderer Fallback (Optional)")]
    [SerializeField] private Renderer fallbackRenderer;

    [Header("Debug")]
    [SerializeField] private bool showHudLabel = false;

    private CompositionLayer compLayer;
    private ColorScaleBiasExtension colorScaleBias;
    private Coroutine fadeRoutine;
    private float cachedAlpha = 0f;
    private bool usingCompositionLayer = false;
    private bool layersHidden = false;

    private void Awake()
    {
        if (compositionLayerGO == null)
            compositionLayerGO = GetComponentInChildren<CompositionLayer>()?.gameObject;

        if (compositionLayerGO != null)
        {
            compLayer = compositionLayerGO.GetComponent<CompositionLayer>();
            colorScaleBias = compositionLayerGO.GetComponent<ColorScaleBiasExtension>();

            var texExt = compositionLayerGO.GetComponent<TexturesExtension>();
            if (compLayer != null && texExt != null)
            {
                Debug.Log("[FadeScreen] Creating default 1x1 black texture.");
                int texSize = 256;
                var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
                Color[] fill = new Color[texSize * texSize];
                for (int i = 0; i < fill.Length; i++) fill[i] = Color.black;
                tex.SetPixels(fill);
                tex.Apply(false, true);

                texExt.sourceTexture = TexturesExtension.SourceTextureEnum.LocalTexture;
                texExt.LeftTexture = tex;
                texExt.RightTexture = tex;
            }
        }

        usingCompositionLayer = ShouldUseCompositionLayer();
        ApplyBackendActivation();
    }

    private IEnumerator Start()
    {
        if (fadeOnStart)
        {
            yield return new WaitForSeconds(0.5f);
            FadeIn();
        }
    }

    // -----------------------------
    // Public API
    // -----------------------------
    public void FadeIn() => Fade(1f, 0f);
    public void FadeOut() => Fade(0f, 1f);

    public void SetFadeDuration(float duration) => fadeDuration = Mathf.Max(0f, duration);

    public void Fade(float alphaIn, float alphaOut)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (usingCompositionLayer)
        {
            if (compositionLayerGO == null || colorScaleBias == null) return;
            fadeRoutine = StartCoroutine(FadeCompositionRoutine(alphaIn, alphaOut));
        }
        else if (fallbackRenderer != null)
        {
            fadeRoutine = StartCoroutine(FadeRendererRoutine(alphaIn, alphaOut));
        }
        else
        {
            Debug.LogWarning("[FadeScreen] No valid fade backend available.");
        }
    }

    public void PerformFade(float targetAlpha)
    {
        if (usingCompositionLayer)
        {
            if (colorScaleBias == null || compLayer == null || compositionLayerGO == null)
            {
                Debug.LogWarning("[FadeScreen] Composition layer not available.");
                return;
            }

            float currentAlpha = colorScaleBias.Bias.w;
            if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeCompositionRoutine(currentAlpha, targetAlpha));
        }
        else if (fallbackRenderer != null)
        {
            if (fallbackRenderer.material == null || !fallbackRenderer.gameObject.activeInHierarchy) return;

            fallbackRenderer.enabled = true;

            if (!fallbackRenderer.material.HasProperty("_Color"))
            {
                Debug.LogError("Material does not have the '_Color' property.");
                return;
            }

            float currentAlpha = fallbackRenderer.material.color.a;
            if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeRendererRoutine(currentAlpha, targetAlpha));
        }
        else
        {
            Debug.LogWarning("[FadeScreen] No valid fade backend available.");
        }
    }

    // -----------------------------
    // Fade Logic
    // -----------------------------

    private IEnumerator FadeCompositionRoutine(float startAlpha, float endAlpha)
    {
        /* TO SHOW MENUS ETC.
        if (compositionLayerGO != null)
            compositionLayerGO.SetActive(true);
        if (compLayer != null)
            compLayer.enabled = true;
        if (fallbackRenderer != null)
            fallbackRenderer.enabled = false;
        */

        float timer = 0f;
        while (timer < fadeDuration)
        {
            float a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            ApplyAlphaScale(a);
            cachedAlpha = a;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyAlphaScale(endAlpha);
        cachedAlpha = endAlpha;


        /* TO SHOW MENUS ETC.
        if (Mathf.Approximately(endAlpha, 1f))
        {
            if (compLayer != null)
                compLayer.enabled = false;
            if (compositionLayerGO != null)
                compositionLayerGO.SetActive(false);
            if (fallbackRenderer != null)
            {
                fallbackRenderer.gameObject.SetActive(true);
                fallbackRenderer.enabled = true;
                var mat = fallbackRenderer.material;
                if (mat.HasProperty("_Color"))
                    mat.color = new Color(0, 0, 0, 1);
            }
        }*/
        fadeRoutine = null;
    }

    private IEnumerator FadeRendererRoutine(float startAlpha, float endAlpha)
    {
        if (!fallbackRenderer.enabled)
            fallbackRenderer.enabled = true;

        float timer = 0f;

        Color baseColor = fallbackRenderer.material.color;
        while (timer < fadeDuration)
        {
            float a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fallbackRenderer.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            cachedAlpha = a;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        fallbackRenderer.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, endAlpha);
        cachedAlpha = endAlpha;

        if (Mathf.Approximately(endAlpha, 0f))
            fallbackRenderer.enabled = false;

        fadeRoutine = null;
    }

    private void ApplyAlphaScale(float alpha)
    {
        if (colorScaleBias == null) return;

        var currentBias = colorScaleBias.Bias;
        currentBias.w = alpha; // only adjust alpha channel
        colorScaleBias.Bias = currentBias;
    }

    private bool ShouldUseCompositionLayer()
    {
        switch (forceBackend)
        {
            case FadeBackend.RendererOnly:
                return false;

            case FadeBackend.CompositionLayerOnly:
                if (compositionLayerGO == null || compLayer == null || colorScaleBias == null)
                {
                    Debug.LogWarning("[FadeScreen] CompositionLayerOnly is set but layer components are missing!");
                    return false;
                }
                return true;

            case FadeBackend.Auto:
            default:
                return (compositionLayerGO != null && compLayer != null && colorScaleBias != null);
        }
    }

    private void ApplyBackendActivation()
    {
        if (usingCompositionLayer)
        {
            if (compositionLayerGO != null && !compositionLayerGO.activeSelf)
                compositionLayerGO.SetActive(true);
            if (compLayer != null) compLayer.enabled = true;

            if (fallbackRenderer != null)
            {
                fallbackRenderer.enabled = false;
                if (fallbackRenderer.gameObject.activeSelf)
                    fallbackRenderer.gameObject.SetActive(false);
            }
        }
        else
        {
            ActivateRendererIfPresent();

            if (compositionLayerGO != null && compositionLayerGO.activeSelf)
                compositionLayerGO.SetActive(false);
            if (compLayer != null) compLayer.enabled = false;
        }
    }

    private void ActivateRendererIfPresent()
    {
        if (fallbackRenderer == null) return;

        if (!fallbackRenderer.gameObject.activeSelf)
            fallbackRenderer.gameObject.SetActive(true);

        fallbackRenderer.enabled = true;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnGUI()
    {
        if (!showHudLabel) return;
        string backend = usingCompositionLayer ? "CompositionLayer" : "Renderer";
        GUI.Box(new Rect(10, 10, 260, 36), GUIContent.none);
        GUI.Label(new Rect(10, 10, 260, 36), $"Fader: {backend}");
    }
#endif
}
