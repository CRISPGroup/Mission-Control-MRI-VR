using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    public Image targetImage;
    public Color newColor = new Color(198f / 255f, 198f / 255f, 198f / 255f);

    public void ChangeImageColor()
    {
        if (targetImage != null)
        {
            Color color = this.newColor;
            targetImage.color = color;
        }
    }

}
