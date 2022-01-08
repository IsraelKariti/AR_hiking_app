using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SightScript : MonoBehaviour
{
   // public Text t;
    public double lat { get { return _lat; } set { _lat = value; } }
    public double lon { get { return _lon; } set { _lon = value; } }
    public float alt { get { return _alt; } set { _alt = value; } }
    public string name { get { return _name; } set { _name = value; } }
    public Camera arCam { set { _arCam = value; } }

    private double _lat;
    private double _lon;
    private float _alt;
    private string _name;
    private Camera _arCam;

    private void Update()
    {
        //1) calculate the position of the user relative to the sight
        Vector3 userPosInSightCoor = transform.InverseTransformPoint(_arCam.transform.position);
        // 2) calculate the angle of the user relative to the sight
        float angle = Mathf.Atan2(-userPosInSightCoor.x, -userPosInSightCoor.z)*Mathf.Rad2Deg;
        // 3) rotate the sight
        transform.Rotate(0, angle, 0);
        //t.text = "userPosInSightCoor: " + userPosInSightCoor + "\nangle: " + angle;
    }
}
