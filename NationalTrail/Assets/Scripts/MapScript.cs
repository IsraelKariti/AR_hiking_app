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
public class MapScript : MonoBehaviour
{
    public double centerLat { get { return _centerLat; } set { _centerLat = value; } }
    public double centerLon { get { return _centerLon; } set { _centerLon = value; } }
    public GpsScript gpsScript;
    public GameObject gpsSamplePrefab;
    public GameObject poiPrefab;
    public Camera arCam;
    public Text textElev;
    public List<GameObject> mapSamples { get { return _samples; } }
   
    private string TAG = "Generate MapScript";
   
    private List<GameObject> _samples;
    // all the way to Oren center

    private double _centerLat;
    private double _centerLon;
    private float defaultAlt = 290;
    //private double widthMeters;//the east<-->west in meters 
    //private double lengthMeters;// the north<-->south in meters
    private List<string> fileLines;
    private List<GameObject> pois;

    private void Awake()
    {

        _samples = new List<GameObject>();// gps samples
        pois = new List<GameObject>();// point of interest(buildings, etc)
        fileLines = new List<string>();

        //TODO:
        //1) read all lines from poi file
        StreamReader reader = new StreamReader(Application.persistentDataPath+"/pois.txt");
        string line;
        while((line = reader.ReadLine())!=null)
        {
            fileLines.Add(line);
            GameObject go = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity, transform);
            pois.Add(go);

            // get all elments in line
            string[] elements = line.Split(' ');
            // set the name of the poi
            go.GetComponent<PoiScript>().poiName = elements[0];
            // set the altitude of the poi
            go.GetComponent<PoiScript>().centerAlt = float.Parse(elements[elements.Length - 1]);
        }
        reader.Close();
        //2) for each line instantiate poi prefab into a list
    }

    // Start is called before the first frame update
    void Start()
    {
        // calculate the center of the tile
        _centerLat = 31.2626509;// (downLeftCornerLat + upRightCornerLat)/ 2;
        _centerLon = 34.7941817;// (downLeftCornerLon + upRightCornerLon)/ 2;

        //TODO:
        //1) for each line in the file create list of 4 tuples and call each poi setCoordinates
        for(int i = 0;i < pois.Count;i++)
        {
            // get all elments in line
            string[] elements = fileLines[i].Split(' ');

            // create the coords
            List<Tuple<double, double>> tuples = new List<Tuple<double, double>>();
            for (int j = 1; j < elements.Length - 1; j++)
            {
                string[] latlon = elements[j].Split(',');
                tuples.Add(new Tuple<double, double>(float.Parse(latlon[0]), float.Parse(latlon[1])));
            }
            pois[i].GetComponent<PoiScript>().setCoordinates(tuples);
        }
      
        // add pois to map
        addPois();

        //TODO:
        //1) set all height with loop from file
        foreach(GameObject go in pois)
        {
            float y = go.GetComponent<PoiScript>().centerAlt - defaultAlt;
            go.transform.position = new Vector3(go.transform.position.x, y, go.transform.position.z);
        }
        

        gpsScript.GpsUpdatedSetMap += OnGpsUpdated;

    }
    private void Update()
    {
        double y = getElevationFromFloor();
        textElev.text = y.ToString("0.0");

    }
    // add all the pois in the map
    private void addPois()
    {
        PoiScript[] allPois = GetComponentsInChildren<PoiScript>();
        foreach (PoiScript child in allPois)
        {
            positionPoiInMap(child);
        }
    }

    private void positionPoiInMap(PoiScript child)
    {
        // get the center of the poi
        double childLat = child.centerLat;
        double childLon = child.centerLon;
        
        // calcula the position of the center of the poi
        double zMeters = GeoToMetersConverter.convertLatDiffToMeters(_centerLat - childLat);
        double xMeters = GeoToMetersConverter.convertLonDiffToMeters(_centerLon - childLon, _centerLat);

        // in this area of the world the positive z axis is opposite direction of the north heading
        // so we add the minus sign to z
        child.gameObject.transform.localPosition = new Vector3(-(float)xMeters, 0, -(float)zMeters);
    }
    

    public void OnGpsUpdated(double lat, double lon, float acc)
    {

        // calculate the x-z of the sample
        Vector3 samplePosition;


        double z = GeoToMetersConverter.convertLatDiffToMeters(_centerLat - lat);
        double x = GeoToMetersConverter.convertLonDiffToMeters(_centerLon - lon , _centerLat);
        double y = getElevationFromFloor();
        samplePosition = new Vector3(-(float)x, 0, -(float)z);
        // calculate the y of the sample
        
        // create the sample 3D text
        GameObject sample = Instantiate(gpsSamplePrefab, Vector3.zero, Quaternion.identity, transform);
        sample.transform.localPosition = samplePosition;
        sample.GetComponentsInChildren<TextMeshPro>()[0].text = acc.ToString("0.0");
        sample.GetComponentsInChildren<TextMeshPro>()[1].text = acc.ToString("0.0");
        _samples.Add(sample);


    }

    private float getElevationFromFloor()
    {
        RaycastHit hit;

        if (Physics.Raycast(arCam.transform.position, -Vector3.up, out hit))
        {
            return hit.distance;
        }
        else
            return 1.1f;// the default height of smartphone above ground while held by user
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
    public void setMapHeight(float h)
    {
        transform.position = new Vector3(transform.position.x, h, transform.position.z);
    }
}
