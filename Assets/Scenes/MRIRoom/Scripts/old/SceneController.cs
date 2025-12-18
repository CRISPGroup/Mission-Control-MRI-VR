using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles scene-level camera transitions and optional screen fade effects.
/// </summary>
/// <remarks>
/// This component can switch between two cameras after a delay and perform fade-in / fade-out
/// effects on a UI Image used as a blackout overlay.  
/// Attach it to an empty GameObject in your scene and assign references in the Inspector.
/// </remarks>
[DisallowMultipleComponent]
public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Camera References")]
    [Tooltip("The first camera that will be disabled when switching.")]
    public GameObject camera1;

    [Tooltip("The second camera that will be enabled when switching.")]
    public GameObject camera2;

    [Header("Fade Settings")]
    [Tooltip("UI Image used to fade the screen in or out (typically a black overlay).")]
    public GameObject blackOutSquare;

    [Tooltip("If true, the fade will go to black. If false, it will fade out to transparent.")]
    public bool fadeToBlack;

    [Tooltip("Speed at which the fade effect occurs.")]
    public float fadeSpeed = 2f;
    void Start()
    {
        Invoke("SwitchCameras", 5);
    }

    /// <summary>
    /// Disables <see cref="camera1"/> and activates <see cref="camera2"/>.
    /// </summary>
    public void SwitchCameras()
    {
        camera1.SetActive(false);
        camera2.SetActive(true);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
           // StartCoroutine(FadeBlackOutSquare());
        }


        if (Input.GetKeyDown(KeyCode.S))
        {
           // StartCoroutine(FadeBlackOutSquare(false));
        }
    }

    /// <summary>
    /// Gradually fades the assigned <see cref="blackOutSquare"/> image to or from black.
    /// </summary>
    /// <returns>Coroutine that yields during the fade process.</returns>
    /// <remarks>
    /// - When <see cref="fadeToBlack"/> is true, the alpha is increased until fully opaque.  
    /// - When false, the alpha decreases until fully transparent.  
    /// </remarks>
    public IEnumerator FadeBlackOutSquare()
    {
        Color objectColor = blackOutSquare.GetComponent<Image>().color;
        float fadeAmount;

        if (fadeToBlack)
        {
            while (blackOutSquare.GetComponent<Image>().color.a < 1)
            {
                fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                yield return null;
            }
        }
        else
        {
            while (blackOutSquare.GetComponent<Image>().color.a > 0)
            {

                fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                yield return null;
            }
        }
    }


}