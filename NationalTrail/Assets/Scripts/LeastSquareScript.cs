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
    public GameObject mapToppings;
    public MapScript mapScript;
    public GroundScript groundScript;
    public GpsScript gpsScript;
    public GameObject collisionDetector;
    public Text shiftEnabledTextIndicator;
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
    //private bool _axOn;
    private Vector3 mapToppingsGlobalPositionBeforeLS;
    private Vector3 mapToppingsLocalPositionBeforeLS;
    private Quaternion mapToppingsGlobalRotationBeforeLS;
    private Vector3 mapToppingsGlobalPositionAfterLS;
    private Vector3 mapToppingsLocalPositionAfterLS;

    private int countGpsSamplesConstantRotation = 0;
    private int countGpsSamplesConstantPositionX = 0;
    private int countGpsSamplesConstantPositionZ = 0;
    private bool mapIsStable = false;
    private bool enableLS = true;
    public bool MapIsStable { get => mapIsStable; set => mapIsStable = value; }
    public bool EnableLS { get { return enableLS; } set { enableLS = value; } }

    private void Start()
    {
        mapSamples = mapScript.mapSamples;
        groundSamples = groundScript.groundSamples;
        gpsScript.GpsUpdatedCalcLeastSquares += OnGpsUpdated;
        File.Delete(Application.persistentDataPath + "/mapPos.txt");
        File.Delete(Application.persistentDataPath + "/gpsForShift.txt");
        File.Delete(Application.persistentDataPath + "/LSTRACKER.txt");

        File.AppendAllText(Application.persistentDataPath + "/mapPos.txt", "Position on LS\n");

    }

    public void OnGpsUpdated()
    {
        // enableLS is false whenever the user is walking parallel to a connector
        if (enableLS)
        {
            Debug.Log(TAG + "LeastSquareScript OnGpsUpdated start");

            // before we start moving the map we need to make sure that if the map toppings is horizontally locked it will not change the toppings global position
            //File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "OnGpsUpdated\n");

            bool isMapToppingsHorizontallyLocked = mapToppings.GetComponent<MapToppingsScript>().IsHorizontalLocked;
            //File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "isMapToppingsHorizontallyLocked" + isMapToppingsHorizontallyLocked+ "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "\n\n\n\n\n" + "Before LS: " + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "" + DateTime.Now + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "map pos: " + map.transform.position + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "map rot: " + map.transform.rotation.eulerAngles + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "isMapToppingsHorizontallyLocked: " + isMapToppingsHorizontallyLocked + "\n");

            if (isMapToppingsHorizontallyLocked)
            {
                mapToppingsGlobalPositionBeforeLS = mapToppings.transform.position;
                mapToppingsLocalPositionBeforeLS = mapToppings.transform.localPosition;
                mapToppingsGlobalRotationBeforeLS = mapToppings.transform.rotation;
                File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings global Position: " + mapToppingsGlobalPositionBeforeLS + "\n");
                File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings local Position: " + mapToppingsLocalPositionBeforeLS + "\n");
                File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings local Rotation: " + mapToppings.transform.localRotation + "\n");
            }


            File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "\n\n\n\n");
            File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "\n\n\n\n");
            File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "" + DateTime.Now + "\n");
            File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");

            //STEP 1: rotate around y
            findBestRotation();

            //STEP 2: move on x-z plane 
            findeBestPosition();

            // STEP 3: if the map is stable enable shifting functionality
            checkStability();

            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "After LS: " + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "" + DateTime.Now + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "map pos: " + map.transform.position + "\n");
            File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "map rot: " + map.transform.rotation.eulerAngles + "\n");

            if (isMapToppingsHorizontallyLocked)
            {
                mapToppingsGlobalPositionAfterLS = mapToppings.transform.position;
                mapToppingsLocalPositionAfterLS = mapToppings.transform.localPosition;
                File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings global Position: " + mapToppingsGlobalPositionAfterLS + "\n");
                File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings local Position: " + mapToppingsLocalPositionAfterLS + "\n");
                File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings local Rotation: " + mapToppings.transform.localRotation + "\n");
                if (isMapToppingsHorizontallyLocked)
                {
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "isMapToppingsHorizontallyLocked: " + isMapToppingsHorizontallyLocked + "\n");

                    // if the toppings position BEFORE the move are still in the gps error radius
                    if (Vector3.Distance(map.transform.position, mapToppingsGlobalPositionBeforeLS) < Values.GPS_ERROR_RADIUS)
                    {
                        File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "still in bounds: " + "\n");

                        // restore the map toppings to be at the previous position and rotation 
                        mapToppings.transform.position = mapToppingsGlobalPositionBeforeLS;
                        mapToppings.transform.rotation = mapToppingsGlobalRotationBeforeLS;
                    }
                    else// if the map has moved too far from the toppings physical\global position than reset the toppings to the center of the map
                    {
                        File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "out of bounds: " + "\n");
                        mapToppings.transform.localPosition = Vector3.zero;
                        mapToppings.GetComponent<MapToppingsScript>().IsHorizontalLocked = false;
                    }
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "After FIX: " + "\n");
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "map Pos: " + map.transform.position + "\n");
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "map rot: " + map.transform.rotation + "\n");
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings global Position: " + mapToppings.transform.position + "\n");
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings local Position: " + mapToppings.transform.localPosition + "\n");
                    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "mapToppings local Rotation: " + mapToppings.transform.localRotation + "\n");

                }
            }
            // after the map is repositioned check if the toppings are in in horizontal lock
            //if (isMapToppingsHorizontallyLocked)
            //{
            //    File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "isMapToppingsHorizontallyLocked: " + isMapToppingsHorizontallyLocked + "\n");

            //    // if the toppings position BEFORE the move are still in the gps error radius
            //    if (Vector3.Distance(map.transform.position, mapToppingsPrevGlobalPosition) < Values.GPS_ERROR_RADIUS)
            //    {
            //        File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "still in bounds: " + "\n");

            //        // restore the map toppings to be at the previous position and rotation 
            //        mapToppings.transform.position = mapToppingsPrevGlobalPosition;
            //    }
            //    else// if the map has moved too far from the toppings physical\global position than reset the toppings to the center of the map
            //    {
            //        File.AppendAllText(Application.persistentDataPath + "/gpsForShift.txt", "out of bounds: " + "\n");
            //        mapToppings.transform.localPosition = Vector3.zero;
            //        mapToppings.GetComponent<MapToppingsScript>().IsHorizontalLocked = false;
            //    }
            //}
        }
    }

    private void checkStability()
    {
        File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "countGpsSamplesConstantRotation: "+ countGpsSamplesConstantRotation + "\n");
        File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "countGpsSamplesConstantPositionX: " + countGpsSamplesConstantPositionX + "\n");
        File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "countGpsSamplesConstantPositionZ: " + countGpsSamplesConstantPositionZ + "\n");

        // check if all conditions for stable map is applied and enable collision detection for shifting
        if (countGpsSamplesConstantRotation >= Values.MIN_GPS_SAMPLES_TO_CONSTANT_MAP_FOR_STABILITY &&
            countGpsSamplesConstantPositionX >= Values.MIN_GPS_SAMPLES_TO_CONSTANT_MAP_FOR_STABILITY &&
            countGpsSamplesConstantPositionZ >= Values.MIN_GPS_SAMPLES_TO_CONSTANT_MAP_FOR_STABILITY)
        {
            mapIsStable = true;
            collisionDetector.SetActive(true);
            shiftEnabledTextIndicator.text = "Y";
            File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "YES" + "\n");
        }
        else
        {
            mapIsStable = false;
            collisionDetector.SetActive(false);
            shiftEnabledTextIndicator.text = "N";
            File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "NO" + "\n");
        }
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
        File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "rotation count: "+counter + "\n");

        //if the angle (around Y axis) stayed the same notify the counter
        if (Math.Abs(counter) <= Values.MIN_THRESHOLD_ROTATION_Y_CONSIDERED_STABLE)
            countGpsSamplesConstantRotation++;
        else
            countGpsSamplesConstantRotation = 0;
    }
    private void findeBestPosition()
    {
        float bestX = findBestPositionOnXAxis();
        float bestZ = findBestPositionOnZAxis();

        Vector3 mapMovement = new Vector3(bestX, 0, bestZ);

        map.transform.position += mapMovement;

        // if the shift in position is stable increase the counter
        if (Mathf.Abs(bestX) <= Values.MIN_THRESHOLD_REPOSITION_X_CONSIDERED_STABLE)
            countGpsSamplesConstantPositionX++;
        else
            countGpsSamplesConstantPositionX = 0;

        if (Mathf.Abs(bestZ) <= Values.MIN_THRESHOLD_REPOSITION_Z_CONSIDERED_STABLE)
            countGpsSamplesConstantPositionZ++;
        else
            countGpsSamplesConstantPositionZ = 0;

        File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "x shift: " + bestX + "\n");
        File.AppendAllText(Application.persistentDataPath + "/LSTRACKER.txt", "z shift: " + bestZ + "\n");
    }

    private float findBestPositionOnXAxis()
    {
        List<GameObject> mapSamples = mapScript.mapSamples;
        List<GameObject> groundSamples = groundScript.groundSamples;
        float totalX = 0;
        float avg = 0;
        int limit = 0;
        File.AppendAllText(Application.persistentDataPath + "/AvgX.txt", "find best x:" + "\n");
        if (mapSamples.Count > 0 && groundSamples.Count >0)
        {
            limit = mapSamples.Count > groundSamples.Count ? groundSamples.Count : mapSamples.Count;
            File.AppendAllText(Application.persistentDataPath + "/AvgX.txt", "limit:" +limit+ "\n");

            for (int i = 0; i < limit; i++)
            {
                File.AppendAllText(Application.persistentDataPath + "/AvgX.txt", "groundSamples[i].transform.position.x - mapSamples[i].transform.position.x:" + groundSamples[i].transform.position.x +"-"+ mapSamples[i].transform.position.x + "\n");

                totalX += (groundSamples[i].transform.position.x - mapSamples[i].transform.position.x);

            }
            File.AppendAllText(Application.persistentDataPath + "/AvgX.txt", "totalX:" + totalX + "\n");

            avg = totalX / limit;
            File.AppendAllText(Application.persistentDataPath + "/AvgX.txt", "avg:" + avg + "\n");

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



    private Vector3 getAxPosition()
    {
        float mapSamplesAvgX = map.GetComponent<MapScript>().getMapSamplesAvgX();
        float mapSamplesAvgZ = map.GetComponent<MapScript>().getMapSamplesAvgZ();
        return new Vector3(mapSamplesAvgX, 0, mapSamplesAvgZ);
    }
}
