using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Changes the color of a specified UI <see cref="Image"/> component.
/// Can be used for visual feedback, UI highlighting, or state indication.
/// </summary>
public class ColorChanger : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The UI Image whose color will be changed.")]
    public Image targetImage;

    [Tooltip("The new color to apply to the target image.")]
    public Color newColor = new Color(198f / 255f, 198f / 255f, 198f / 255f);

    /// <summary>
    /// Applies the configured color to the target image, if assigned.
    /// </summary>
    public void ChangeImageColor()
    {
        if (targetImage != null)
        {
            Color color = this.newColor;
            targetImage.color = color;
        }
    }

}
