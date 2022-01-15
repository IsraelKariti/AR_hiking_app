using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
// this class represents a constant rectangular area (Tile) 
// pois in the area will be the children of this game object
// this map ALTITUDE at y = 0 is 290 meters
// the variables that define this map is: lat+lon+alt
public class MapScript : MonoBehaviour
{
    public GpsScript gpsScript;
    public GameObject gpsSamplePrefab;
    public Camera arCam;


    public double MapCenterLat { get { return mapcenterLat; } set { mapcenterLat = value; } }
    public double MapCenterLon { get { return mapcenterLon; } set { mapcenterLon = value; } }
    public float MapCenterAlt { get => mapCenterAlt; set => mapCenterAlt = value; }
    public List<GameObject> mapSamples { get { return _samples; } }

    private List<GameObject> _samples;
    // all the way to Oren center

    private double mapcenterLat;
    private double mapcenterLon;
    private float mapCenterAlt;
    private bool isMapCenterIntialized = false;
    private void Awake()
    {
        _samples = new List<GameObject>();// gps samples
        File.Delete(Application.persistentDataPath + "/mapLog.txt");
    }
    // Start is called before the first frame update
    void Start()
    {
        gpsScript.GpsUpdatedSetMap += OnGpsUpdated;
    }
    public void OnGpsUpdated(double lat, double lon, float acc)
    {

        if (!isMapCenterIntialized)
        {
            File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "!isMapToppingsIntialized\n");

            // initialize the LAT+LON+ALT of the map to be from the first poi

            mapcenterLat = lat;
            mapcenterLon = lon;
            isMapCenterIntialized = true;
        }
        File.WriteAllText(Application.persistentDataPath + "/mapLog.txt", "OnGpsUpdated");

        // calculate the x-z of the sample
        Vector3 samplePosition;

        double z = GeoToMetersConverter.convertLatDiffToMeters(mapcenterLat - lat);
        double x = GeoToMetersConverter.convertLonDiffToMeters(mapcenterLon - lon, mapcenterLat);
        samplePosition = new Vector3(-(float)x, 0, -(float)z);
        // calculate the y of the sample

        // create the sample 3D text
        GameObject sample = Instantiate(gpsSamplePrefab, Vector3.zero, Quaternion.identity, transform);
        sample.transform.localPosition = samplePosition;
        // this is redundant i don't use the acc anymore, it's not accurate itself
        //foreach(TextMeshPro tmp in sample.GetComponentsInChildren<TextMeshPro>())
        //{
        //    tmp.text = acc.ToString("0.0");
        //}
        _samples.Add(sample);
    }

    public float getMapSamplesAvgX()
    {
        float sumX = 0;
        float avgX = 0;
        
        foreach (GameObject go in mapSamples)
        {
            sumX += go.transform.position.x;
        }

        avgX = sumX / mapSamples.Count;
        return avgX;
    }    
    
    public float getMapSamplesAvgZ()
    {
        float sumZ = 0;
        float avgZ = 0;
        foreach(GameObject go in mapSamples)
        {
            sumZ += go.transform.position.z;
        }
        avgZ = sumZ / mapSamples.Count;
        return avgZ;
    }
}
