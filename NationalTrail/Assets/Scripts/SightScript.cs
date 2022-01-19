using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SightScript : MonoBehaviour
{
   // public Text t;
    public double Lat { get { return _lat; } set { _lat = value; } }
    public double Lon { get { return _lon; } set { _lon = value; } }
    public float Alt { get { return _alt; } set { _alt = value; } }
    public string Name { get { return name; } set { name = value; } }
    public Camera arCam { set { _arCam = value; } get { return _arCam; } }

    private double _lat;
    private double _lon;
    private float _alt;
    private string name;
    private Camera _arCam;
    private float h;
    private float w;
    private int sightLayerMask = 1 << 3;
    public bool isPositioned = false;
    private void Awake()
    {
        File.Delete(Application.persistentDataPath + "/reheight.txt");
        File.Delete(Application.persistentDataPath + "/log.txt");
    }

    public void setSightOnMap() {
        positionSightOnMap();
        beautifySightText();
    }

    private void positionSightOnMap()
    {
        MapToppingsScript mapToppingsScript = transform.parent.GetComponent<MapToppingsScript>();
        // calcula the position of the center of the poi
        double zMeters = GeoToMetersConverter.convertLatDiffToMeters(mapToppingsScript.MapCenterLat - Lat);
        double xMeters = GeoToMetersConverter.convertLonDiffToMeters(mapToppingsScript.MapCenterLon - Lon, mapToppingsScript.MapCenterLat);
        double yMeters = Alt - mapToppingsScript.MapCenterAlt;
        // position the sight in the map
        transform.localPosition = new Vector3(-(float)xMeters, (float)yMeters, -(float)zMeters);
    }
    // this will change the font and background size
    private void beautifySightText()
    {
        float dist = Vector3.Distance(_arCam.transform.position, transform.position);
        TMP_Text text = GetComponent<TMP_Text>();

        // adjust text font
        text.fontSize += dist / 6f;

        // retrieve the preffered dimensions for the adjusted font size
        float preferedHeight = text.preferredHeight;
        float preferedWidth = text.preferredWidth;
        w = preferedWidth;
        h = preferedHeight;
        // adjust text container size
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector3(preferedWidth, preferedHeight, 0.1f);
        // adjust cube (background) size
        Transform background = transform.GetChild(0);
        background.localScale = new Vector3(preferedWidth + 10, preferedHeight, 0.1f);
        // adjust cube (frame) size
        Transform frame = transform.GetChild(1);
        frame.localScale = new Vector3(preferedWidth + 20, preferedHeight + 10, 0.1f);

    }
    private void Update()
    {
        if(isPositioned)
           rotateTowardCam();
    }

    private void rotateTowardCam()
    {
        //1) calculate the position of the user relative to the sight
        Vector3 userPosInSightCoor = transform.InverseTransformPoint(_arCam.transform.position);
        // 2) calculate the angle of the user relative to the sight
        float angle = Mathf.Atan2(-userPosInSightCoor.x, -userPosInSightCoor.z) * Mathf.Rad2Deg;
        // 3) rotate the sight
        transform.Rotate(0, angle, 0);
        //t.text = "userPosInSightCoor: " + userPosInSightCoor + "\nangle: " + angle;    }
    }


    // this function is called from MapScript every second (in the order of sights in the list)
    // this function responsible for moving a sign/sight up or down so that the signs won't hide each other
    public void Reheight()
    {
        File.AppendAllText(Application.persistentDataPath + "/reheight.txt", "reheigt: "+gameObject+"\n");
        File.AppendAllText(Application.persistentDataPath + "/reheight.txt", ""+DateTime.Now+"\n");

        bool flag = false;
        Vector3 originalPosition;
        // check if the sign is hidden, than define original designated position)
        if (checkIfCornersHiddenToOrigin())
        {
            File.AppendAllText(Application.persistentDataPath + "/reheight.txt", "hidden"  + "\n");

            // save the original position (where the sight is suppose to be if it didn't need to move)
            transform.position = originalPosition = new Vector3(transform.position.x, _alt - transform.parent.GetComponent<MapToppingsScript>().MapCenterAlt, transform.position.z);
        }
        else
        {
            File.AppendAllText(Application.persistentDataPath + "/reheight.txt", "visible" + "\n");

            return;
        }

        // check how HIGH the sign needs to be lifted to be completely visible to the camera
        while (checkIfCornersHiddenToOrigin())
        {
            flag = true;
            transform.position += Vector3.up;
        }
        //FOR NOW SETTLE FOR UPWARD INCREMENTATION~~~
        //if the sight had to be moved up check if it's better to move down
        if (flag)
        {
            // save the y value of the sign when it is visible to the camera after moving upwards
            Vector3 increasedPosition = transform.position;

            // reset the y to the original value
            transform.position = originalPosition;
            // check how LOW the sign needs to be lifted to be completely visible to the camera
            while (checkIfCornersHiddenToOrigin())
            {
                transform.position -= Vector3.up;
            }
            Vector3 decreasedPosition = transform.position;

            // check which direction the sign has to move to the least to be visible
            if (Mathf.Abs(increasedPosition.y - originalPosition.y) <= Mathf.Abs(decreasedPosition.y - originalPosition.y))
            {
                transform.position = increasedPosition;
            }
            else
            {
                transform.position = decreasedPosition;
            }
        }
    }

    // check if all 4 corners of sight is visible to origin
    private bool checkIfCornersHiddenToOrigin()
    {
        string log= ""+ DateTime.Now+" "+name + " pos: " + transform.position.y+" ";
        RaycastHit hit;
        bool flag = false;
        Vector3[] corners = getCorners();
        BoxCollider colliderBackground = transform.GetChild(0).GetComponent<BoxCollider>();
        BoxCollider colliderFrame = transform.GetChild(1).GetComponent<BoxCollider>();
        foreach (Vector3 corner in corners)
        {
            log += "corner: " + corner;
            // if the ray hit something than it's bad news, cause it's suppose to hit a milimeter before the correct corner
            if (Physics.Raycast(_arCam.transform.position, (corner - _arCam.transform.position), out hit, Mathf.Infinity, sightLayerMask))
            {
                log += " hitDist: " + hit.distance;
                // check if the ray hit different collider from this sight colliders 
                if (hit.collider != colliderBackground && hit.collider != colliderFrame)
                {
                    flag = true;// true means is hidden
                    break;
                }

            }
            else
                log += " NoHit ";
        }
        File.AppendAllText(Application.persistentDataPath + "/log.txt", log + "\n");

        return flag;
    }

    private Vector3[] getCorners()
    {
        return new Vector3[] { transform.TransformPoint(new Vector3(0, 0, 2)) ,
                            transform.TransformPoint(new Vector3(w/2, h/2, 2)),
                            transform.TransformPoint(new Vector3(w/2, -h/2, 2)),
                            transform.TransformPoint(new Vector3(-w/2, h/2, 2)),
                            transform.TransformPoint(new Vector3(-w/2, -h/2, 2))};
    }
}
