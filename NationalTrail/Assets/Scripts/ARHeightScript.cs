using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARHeightScript : MonoBehaviour
{
    public Text text;
    public Camera arCam;
    // Update is called once per frame
    void Update()
    {
        text.text = "AR Y:\n" + arCam.transform.position.y.ToString("0.00");
    }
}
