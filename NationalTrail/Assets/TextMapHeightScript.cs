using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextMapHeightScript : MonoBehaviour
{
    public Text text;
 
    public void setText(float f)
    {
        text.text = f.ToString("0.0");
    }
}
