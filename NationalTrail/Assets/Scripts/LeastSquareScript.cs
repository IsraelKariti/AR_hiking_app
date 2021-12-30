using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LeastSquareScript : MonoBehaviour
{
    public GameObject map;
    public MapScript mapScript;
    public GroundScript groundScript;
    public GpsScript gpsScript;
    public GameObject AxGameObject;
    //public GameObject LinePrefab;
    public bool axOn { set { _axOn = value; } }
    public Text text;

    private string TAG = "LeastSquareScript";
    private List<GameObject> mapSamples;
    private List<GameObject> groundSamples;

    //rotation variables
    private float prevLS;
    private int rotateDirection;// 0 : dont rotate      -1 : rotate CW       +1 : rotate CCW
    private float currLS;
    private int counter;
    private Vector3 axVector3;
    private float prevAngle;
    private bool _axOn;
    private void Start()
    {
        mapSamples = mapScript.mapSamples;
        groundSamples = groundScript.groundSamples;
        gpsScript.GpsUpdatedCalcLeastSquares += OnGpsUpdated;
        _axOn = true;
    }
    public void Update()
    {
        AxGameObject.SetActive(_axOn);
    }

    

    public void OnGpsUpdated()
    {
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated start");
        

        //STEP 1: rotate around y
        findBestRotation();

        //STEP 2: move on x-z plane 
        findeBestPosition();
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated end a");


        AxGameObject.transform.position = getAxPosition();

    }

    public void findBestRotation()
    {

        axVector3 = getAxPosition();


        //AxGameObject.transform.position = axVector3;// this line is useless as the whole map is about to change rotation and than position leaging the ax irrelevent(the ax is only relevent before the map mpves)

        //STEP 1: calculate the LS in CCW 
        map.transform.RotateAround(axVector3, Vector3.up, 1);

        float ccwLS = sumSquares();

        //STEP 1: calculate the LS in CW 
        map.transform.RotateAround(axVector3, Vector3.up, -2);
        float cwLS = sumSquares();

        // rotate back to the middle
        map.transform.RotateAround(axVector3, Vector3.up, 1);
        float middleLS = sumSquares();
        prevLS = middleLS;

        //STEP 2: decide CW or CCW
        if (middleLS < cwLS && middleLS < ccwLS)
        {
            // there is nothing to change
            rotateDirection = 0;
        }
        else if (cwLS < middleLS)
        {
            rotateDirection = -1;

        }
        else
        {
            rotateDirection = 1;

        }

        // rotate the GO untill the least squares found
        currLS = middleLS;
        counter = 0;

        do
        {
            prevLS = currLS;
            map.transform.RotateAround(axVector3, Vector3.up, rotateDirection);
            currLS = sumSquares();
            counter += rotateDirection;

        }
        while (currLS < prevLS);

        // rotate back 1 degree
        map.transform.RotateAround(axVector3, Vector3.up, -1 * rotateDirection);
        counter += (-1 * rotateDirection);
        currLS = prevLS;

    }
    private void findeBestPosition()
    {
        float bestX = findBestPositionOnXAxis();
        float bestZ = findBestPositionOnZAxis();
        Vector3 mapMovement = new Vector3(bestX, 0, bestZ);

        map.transform.position += mapMovement;

    }

    private float findBestPositionOnXAxis()
    {
        List<GameObject> mapSamples = mapScript.mapSamples;
        List<GameObject> groundSamples = groundScript.groundSamples;
        float totalX = 0;
        float avg = 0;
        int limit = 0;

        if (mapSamples.Count > 0 && groundSamples.Count >0)
        {
            limit = mapSamples.Count > groundSamples.Count ? groundSamples.Count : mapSamples.Count;
            for (int i = 0; i < limit; i++)
            {
                totalX += (groundSamples[i].transform.position.x - mapSamples[i].transform.position.x);

            }
            avg = totalX / limit;

        }
        return avg;
    }
    private float findBestPositionOnZAxis()
    {
        List<GameObject> mapSamples = mapScript.mapSamples;
        List<GameObject> groundSamples = groundScript.groundSamples;
        float totalZ = 0;
        float avg = 0;
        int limit = 0;

        if (mapSamples.Count > 0 && groundSamples.Count > 0)
        {
            limit = mapSamples.Count > groundSamples.Count ? groundSamples.Count : mapSamples.Count;
            for (int i = 0; i < limit; i++)
            {
                totalZ += (groundSamples[i].transform.position.z - mapSamples[i].transform.position.z);

            }
            avg = totalZ / limit;

        }
        return avg;
    }

    public float sumSquares()
    {

        float sum = 0;
        
        int minSamples = groundSamples.Count < mapSamples.Count ? groundSamples.Count : mapSamples.Count;

        for (int i = 0;i < minSamples;i++)
        {

            //sum += Mathf.Pow( Vector3.Distance(groundSamples[i].transform.position, mapSamples[i].transform.position), 2);

            sum += (Mathf.Pow(groundSamples[i].transform.position.x - mapSamples[i].transform.position.x, 2) + Mathf.Pow(groundSamples[i].transform.position.z - mapSamples[i].transform.position.z, 2));
        }


        return sum;
    }

    public void incAngle()
    {
        Vector3 axVector3 = getAxPosition();
        AxGameObject.transform.position = axVector3;
        map.transform.RotateAround(axVector3, Vector3.up, 1);
        text.text = "LS: " + sumSquares();

    }

    public void decAngle()
    {

        Vector3 axVector3 = getAxPosition();

        AxGameObject.transform.position = axVector3;

        map.transform.RotateAround(axVector3, Vector3.up, -1);

        text.text = "LS: " + sumSquares();

    }

    private Vector3 getAxPosition()
    {
        float mapSamplesAvgX = map.GetComponent<MapScript>().getMapSamplesAvgX();
        float mapSamplesAvgZ = map.GetComponent<MapScript>().getMapSamplesAvgZ();
        return new Vector3(mapSamplesAvgX, 0, mapSamplesAvgZ);
    }
}
