using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchMoonScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("MoonTravel", LoadSceneMode.Additive);
    }
}
