using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeastSquareScript : MonoBehaviour
{
    public GameObject map;
    public MapScript mapScript;
    public GroundScript groundScript;
    public GpsScript gpsScript;
    public TMP_Text text;

    private string TAG = "LeastSquareScript";
    private List<GameObject> mapSamples;
    private List<GameObject> groundSamples;
    private bool init = true;
    //rotation variables
    private Quaternion from;
    private Quaternion to;
    private float prevLS;
    private int rotateDirection;// 0 : dont rotate      -1 : rotate CW       +1 : rotate CCW
    private float currLS;
    private int counter;
    private bool isRotating = false;
    private float accuTime = 0;
    private Vector3 ax;
    private float prevAngle;
    private void Start()
    {

        mapSamples = mapScript.mapSamples;

        groundSamples = groundScript.groundSamples;

        gpsScript.GpsUpdated += OnGpsUpdated;

    }

    public void OnGpsUpdated(float lat, float lon)
    {
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated start");

        //STEP 1: rotate around y
        findBestRotation();

        //STEP 2: move on x-z plane 
        findeBestPosition();
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated end a");

    }

    private void findeBestPosition()
    {
        float bestX = findBestPositionOnXAxis();
        float bestZ = findBestPositionOnZAxis();
        Vector3 mapMovement = new Vector3(bestX, 0, bestZ);
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated findeBestPosition map.transform.position " + map.transform.position);
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated findeBestPosition mapMovement " + mapMovement);

        map.transform.position += mapMovement;

    }

    private float findBestPositionOnXAxis()
    {
        List<GameObject> mapSamples = mapScript.mapSamples;
        List<GameObject> groundSamples = groundScript.groundSamples;
        float totalX = 0;
        float avg = 0;
        int limit = 0;
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated mapSamplesXList.Count "+ mapSamples.Count);
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated groundSamplesXList.Count "+ groundSamples.Count);

        if (mapSamples.Count > 0 && groundSamples.Count >0)
        {
            limit = mapSamples.Count > groundSamples.Count ? groundSamples.Count : mapSamples.Count;
            for (int i = 0; i < limit; i++)
            {
                totalX += (groundSamples[i].transform.position.x - mapSamples[i].transform.position.x);
                Debug.Log(TAG + "LeastSquareScript OnGpsUpdated findBestPositionOnXAxis totalX "+totalX+ " +groundSamplesXList[i] + " +groundSamples[i].transform.position.x + " mapSamplesXList[i] " + mapSamples[i].transform.position.x  );

            }
            avg = totalX / limit;
            Debug.Log(TAG + "LeastSquareScript OnGpsUpdated findBestPositionOnXAxis avg " + avg + "  limit: "+limit);
            Debug.Log(TAG + "final");

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
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated mapSamplesZList.Count " + mapSamples.Count);
        Debug.Log(TAG + "LeastSquareScript OnGpsUpdated groundSamplesZList.Count " + groundSamples.Count);

        if (mapSamples.Count > 0 && groundSamples.Count > 0)
        {
            limit = mapSamples.Count > groundSamples.Count ? groundSamples.Count : mapSamples.Count;
            for (int i = 0; i < limit; i++)
            {
                totalZ += (groundSamples[i].transform.position.z - mapSamples[i].transform.position.z);
                Debug.Log(TAG + "LeastSquareScript OnGpsUpdated findBestPositionOnXAxis totalX " + totalZ + " +groundSamplesZList[i] + " + groundSamples[i].transform.position.z + " mapSamplesZList[i] " + mapSamples[i].transform.position.z);

            }
            avg = totalZ / limit;
            Debug.Log(TAG + "LeastSquareScript OnGpsUpdated findBestPositionOnZAxis avg " + avg + "  limit: " + limit);
            Debug.Log(TAG + "final");

        }
        return avg;
    }

    public void findBestRotation()
    {
        // find rotation axis: center of points
        float mapSamplesAvgX = map.GetComponent<MapScript>().getMapSamplesAvgX();
        float mapSamplesAvgZ = map.GetComponent<MapScript>().getMapSamplesAvgZ();
        ax = new Vector3(mapSamplesAvgX, 0, mapSamplesAvgZ);
        Debug.Log("rotate : center of samples x " + mapSamplesAvgX + " z " + mapSamplesAvgZ);

        //STEP 1: calculate the LS in CCW 
        //map.transform.Rotate(new Vector3(0, 1, 0));
        map.transform.RotateAround(ax, Vector3.up, 1);
        float ccwLS = sumSquares();

        //STEP 1: calculate the LS in CW 
        //map.transform.Rotate(new Vector3(0, -2, 0));
        map.transform.RotateAround(ax, Vector3.up, -2);
        float cwLS = sumSquares();

        // rotate back to the middle
        //map.transform.Rotate(new Vector3(0, 1, 0));
        map.transform.RotateAround(ax, Vector3.up, 1);
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
            //map.transform.Rotate(new Vector3(0, rotateDirection, 0));
            map.transform.RotateAround(ax, Vector3.up, rotateDirection);
            currLS = sumSquares();
            counter += rotateDirection;
        }
        while (currLS < prevLS);

        // rotate back 1 degree
        //transform.Rotate(new Vector3(0, 0, -1 * rotateDirection));
        map.transform.RotateAround(ax, Vector3.up, -1*rotateDirection);
        counter += (-1 * rotateDirection);
        currLS = prevLS;

    }
    private void MapPositionApproximation()
    {
        Debug.Log(TAG + " OnGpsUpdated size "+ groundScript.groundSamplesXList.Count + " init " + init);

            // get the first map samples on the x-z plane
            float moveX = groundScript.groundSamplesXList[0] - mapScript.mapSamplesXList[0];
            float moveZ = groundScript.groundSamplesZList[0] - mapScript.mapSamplesZList[0];
            Debug.Log(TAG + "moveX: " + moveX + " moveZ: " + moveZ);
            map.transform.position += new Vector3(moveX, 0, moveZ);
           
    }

    public float sumSquares()
    {
        float sum = 0;
        for(int i = 0;i < groundSamples.Count;i++)
        {
            sum += Mathf.Pow( Vector3.Distance(groundSamples[i].transform.position, mapSamples[i].transform.position), 2);
        }
        
        return sum;
    }
   
}
