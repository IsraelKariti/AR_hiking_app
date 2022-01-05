using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColorScript : MonoBehaviour
{
    public Material mat;
    private int colorIndex=0;
    private Color[] colors;
    private float alpha = 0.2f;
    private void Start()
    {
        colors = new Color[] { new Color(1, 0, 0, alpha) // red
                                ,new Color(0,1,0,alpha)//green
                                ,new Color(0,0,1,alpha)//blue
                                ,new Color(1,0.7f,0.3f,alpha)//orange
        };
    }
    // i only want to change the color and NOT the entire material because i don't want to loose the sprite (arrow)
    public void changeMatColor()
    {
        colorIndex++;
        Color temp = colors[colorIndex%colors.Length];

        mat.SetColor("_Color", new Color(temp.r,temp.g,temp.b, alpha));
    }
}
