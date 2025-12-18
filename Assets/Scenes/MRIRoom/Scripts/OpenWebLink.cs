using UnityEngine;

/// <summary>
/// Provides a simple method to open a web link from a UI button or event in Unity.
/// </summary>
/// <remarks>
/// This script can be attached to a GameObject and linked to UI Button events.
/// When the button is pressed, it will open the specified URL in the system’s default web browser.
/// </remarks>
public class OpenWebLink : MonoBehaviour
{
    /// <summary>
    /// Opens the given URL in the default web browser.
    /// </summary>
    /// <param name="url">The web address to open (must start with "http://" or "https://").</param>
    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
