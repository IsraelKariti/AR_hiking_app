using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PoiConnectorScript : MonoBehaviour
{
    private Camera arCam;
    private bool isPositioned = false;
    public Camera ArCam { get { return arCam; } set { arCam = value; } }
    private void Awake()
    {
        File.Delete(Application.persistentDataPath + "/rotateConnector.txt");

    }
    public void positionInMap(GameObject go1, GameObject go2)
    {
        // get the location of the connector between pois
        float x = (go1.transform.localPosition.x + go2.transform.localPosition.x) / 2;
        float y = (go1.transform.localPosition.y + go2.transform.localPosition.y) / 2;
        float z = (go1.transform.localPosition.z + go2.transform.localPosition.z) / 2;
        transform.localPosition = new Vector3(x, y, z);

        float poiDist = Vector3.Distance(go1.transform.localPosition, go2.transform.localPosition);
        GetComponentInChildren<SpriteRenderer>().size = new Vector2(1, poiDist);
        GetComponent<BoxCollider>().size = new Vector3(Values.CONNECTOR_COLLIDER_DIAMETER, poiDist-2, Values.CONNECTOR_COLLIDER_DIAMETER);

        // calculate angle of rotation arount x
        // 1) calc dist on x z plane
        float distXZ = Vector3.Distance(new Vector3(go1.transform.localPosition.x, 0, go1.transform.localPosition.z), new Vector3(go2.transform.localPosition.x, 0, go2.transform.localPosition.z));

        // 2) calc dist on y plane
        float heightY = go2.transform.localPosition.y - go1.transform.localPosition.y;

        // 3) calc angle to rotate around x axis
        float rotX = Mathf.Atan2(distXZ, heightY) * Mathf.Rad2Deg;

        float rotY = Mathf.Atan2(go2.transform.localPosition.x - go1.transform.localPosition.x, go2.transform.localPosition.z - go1.transform.localPosition.z) * Mathf.Rad2Deg;

        transform.localRotation = Quaternion.Euler(rotX, rotY, 0);

        isPositioned = true;
    }

    private void Update()
    {
        if (isPositioned)
        {
            adjustConnectorColor();
            adjustConnectorRotation();
        }
        
    }

    private void adjustConnectorRotation()
    {// get the child
        //File.AppendAllText(Application.persistentDataPath + "/rotateConnector.txt", "adjustConnectorRotation\n");
        GameObject child = transform.GetChild(0).gameObject;
        // get the world position of the camera
        Vector3 camPos = arCam.transform.position;

        // get the position of the camera in the child frame of reference
        Vector3 camInConnector = transform.InverseTransformPoint(camPos);
        // calc the angle of rotation between the connector and the camera
        float rotY = Mathf.Atan2(-camInConnector.x, -camInConnector.z) * Mathf.Rad2Deg;

        int HorizonHigh;
        int HorizonLow;
        int responsiveRadius = 20;
        // make the rotation threshold less responsive for closer connectors
        // because they take so much screen space the you can notice them easily even from low angle
        if (Vector3.Distance(child.transform.position, arCam.transform.position) < responsiveRadius)
        {
            HorizonHigh = 75;
            HorizonLow = 135;
        }
        else
        {// make the rotation threshold more responsive for farther connectors
         // because they take so little screen space the you can't notice them from less than 45 degree angle
            HorizonHigh = 45;
            HorizonLow = 135;
        }
        float rotYPhase = 0;
        if (rotY < HorizonHigh && rotY > -HorizonHigh)
        {
            rotYPhase = 0;
        }
        else if (rotY > HorizonHigh && rotY < HorizonLow)
        {
            rotYPhase = 90;
        }
        else if (rotY > HorizonLow || rotY < -HorizonLow)
        {
            rotYPhase = 180;
        }
        else if (rotY < -HorizonHigh && rotY > -HorizonLow)
        {
            rotYPhase = -90;
        }
        child.transform.localRotation = Quaternion.Euler(0, rotYPhase, 0);
    }

    private void adjustConnectorColor()
    {
        // get the connector
        GameObject child = transform.GetChild(0).gameObject;

        // get the world position of the camera
        Vector3 camPos = arCam.transform.position;

        // check dist cam-connector
        float dist = Vector3.Distance(camPos, child.transform.position);

        // take the base material of the game object
        ArrowScript arrowScript = child.GetComponent<ArrowScript>();
        Material baseMaterial = arrowScript.baseMaterial;
        // the base color of the entire scene connectors
        Color baseColor = baseMaterial.color;
        // change color alpha
        baseColor.a = 0.2f + dist / 100.0f;

        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        // create local material
        Material localMat = renderer.material;// PROBLEMATIC LINE - creates a local copy
        // change the local material
        localMat.SetColor("_Color", baseColor);
    }

    
}
