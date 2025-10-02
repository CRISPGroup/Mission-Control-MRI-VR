using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject camera1;
    public GameObject camera2;
    public GameObject blackOutSquare;
    public bool fadeToBlack;
    public float fadeSpeed = 2f;
    void Start()
    {
        Invoke("SwitchCameras", 5);
    }

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