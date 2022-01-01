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
    public double centerLat { get { return _centerLat; } set { _centerLat = value; } }
    public double centerLon { get { return _centerLon; } set { _centerLon = value; } }
    public GpsScript gpsScript;
    public GameObject gpsSamplePrefab;
    public GameObject poiPrefab;
    public Camera arCam;
    public Text textElev;
    public Text dynamicHeightText;
    public GameObject poiConnectorPrefab;

    public List<GameObject> mapSamples { get { return _samples; } }
   
    private string TAG = "Generate MapScript";
   
    private List<GameObject> _samples;
    // all the way to Oren center

    private double _centerLat;
    private double _centerLon;
    private float defaultAlt = 290f;
    //private double widthMeters;//the east<-->west in meters 
    //private double lengthMeters;// the north<-->south in meters
    private List<string> fileLines;
    private List<GameObject> pois;
    private List<GameObject> poiConnectors;

    private bool initCenter;

    private void Awake()
    {
        initCenter = false;
        _samples = new List<GameObject>();// gps samples
        pois = new List<GameObject>();// point of interest(buildings, etc)
        poiConnectors = new List<GameObject>();// point of interest(buildings, etc)
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
            // initialize the LAT+LON+ALT of the map to be from the first poi
            if(initCenter == false)
            {
                _centerLat = float.Parse(elements[1].Split(',')[0]);
                _centerLon = float.Parse(elements[1].Split(',')[1]);
                defaultAlt = float.Parse(elements[elements.Length - 1]);
                initCenter = true;
            }
        }
        reader.Close();
        //2) for each line instantiate poi prefab into a list
    }

    

    // Start is called before the first frame update
    void Start()
    {
        
        // calculate the center of the tile
        //_centerLat = 32.08250;// (downLeftCornerLat + upRightCornerLat)/ 2;
        //_centerLon = 34.77405;// (downLeftCornerLon + upRightCornerLon)/ 2;

        //TODO:
        //1) for each line in the file create list of 4 tuples and call each poi setCoordinates
        for(int i = 0;i < pois.Count;i++)
        {
            // get all elments in line
            string[] elements = fileLines[i].Split(' ');
            Debug.Log("elementis 0: " + elements[0]) ;

            // create the coords
            List<Tuple<double, double>> tuples = new List<Tuple<double, double>>();
            for (int j = 1; j < elements.Length - 1; j++)
            {
                Debug.Log("elementis "+j+": " + elements[j]);

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

        // connect all pois to a route\path
        connectPois();

        gpsScript.GpsUpdatedSetMap += OnGpsUpdated;

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
    // this method is for create the route the hiker is following
    private void connectPois()
    {
        // assuming the pois are in their order of walking 
        for (int i = 0; i < pois.Count - 1; i++)
        {
            Debug.Log("pois pois[i].transform.localPosition.x " + pois[i].transform.localPosition.x);
            Debug.Log("pois pois[i].transform.localPosition.y " + pois[i].transform.localPosition.y);
            Debug.Log("pois pois[i].transform.localPosition.z " + pois[i].transform.localPosition.z);
            Debug.Log("pois pois[i+1].transform.localPosition.x " + pois[i+1].transform.localPosition.x);
            Debug.Log("pois pois[i+1].transform.localPosition.y " + pois[i+1].transform.localPosition.y);
            Debug.Log("pois pois[i+1].transform.localPosition.z " + pois[i+1].transform.localPosition.z);
            // get the location of the connector between pois
            float x = (pois[i].transform.localPosition.x + pois[i + 1].transform.localPosition.x) / 2;
            float y = (pois[i].transform.localPosition.y + pois[i + 1].transform.localPosition.y) / 2;
            float z = (pois[i].transform.localPosition.z + pois[i + 1].transform.localPosition.z) / 2;
            GameObject go = Instantiate(poiConnectorPrefab, Vector3.zero, Quaternion.identity, transform);
            poiConnectors.Add(go);
            // locate the connector between the two pois
            go.transform.localPosition = new Vector3(x, y, z);
            Debug.Log("pois go.transform.localPosition " + go.transform.localPosition.ToString());

            go.transform.localScale = new Vector3(0.01f, Vector3.Distance(pois[i].transform.localPosition, pois[i + 1].transform.localPosition), 1);
            //go.GetComponent<SpriteRenderer>().size = new Vector2(1, Vector3.Distance(pois[i].transform.localPosition, pois[i + 1].transform.localPosition));
            Debug.Log("pois go.transform.localScale " + go.transform.localScale.ToString());

            // calculate angle of rotation arount x
            // 1) calc dist on x z plane
            float distXZ = Vector3.Distance(new Vector3(pois[i].transform.localPosition.x, 0, pois[i].transform.localPosition.z), new Vector3(pois[i + 1].transform.localPosition.x, 0, pois[i + 1].transform.localPosition.z));
            Debug.Log("pois distXZ " + distXZ);

            // 2) calc dist on y plane
            float heightY = pois[i + 1].transform.localPosition.y - pois[i].transform.localPosition.y;
            Debug.Log("pois heightY " + heightY);

            // 3) calc angle to rotate around x axis
            float rotX = Mathf.Atan2(distXZ,heightY)*Mathf.Rad2Deg;
            Debug.Log("pois rotX " + rotX);

            float rotY = Mathf.Atan2(pois[i+1].transform.localPosition.x - pois[i].transform.localPosition.x, pois[i+1].transform.localPosition.z - pois[i].transform.localPosition.z)*Mathf.Rad2Deg;
            Debug.Log("pois rotY " + rotY);

            go.transform.localRotation = Quaternion.Euler(0, rotY+90, rotX);

        }
    }
    public void rotateConnector(float f)
    {
        poiConnectors[0].transform.localRotation = Quaternion.Euler(poiConnectors[0].transform.localRotation.x, poiConnectors[0].transform.localRotation.y, f);
    }
    private void Update()
    {
        //double y = getElevationFromFloor();
        //textElev.text = y.ToString("0.0");

        // loop on all pois find 2 closest
        GameObject minGo1 = pois[0];
        float dist1 = 999999;
        GameObject minGo2 = pois[1];
        float dist2 = 999999;
        foreach (GameObject go in pois)
        {
            float distSqrd = Mathf.Pow(arCam.transform.position.x - go.transform.position.x, 2) + Mathf.Pow(arCam.transform.position.z - go.transform.position.z, 2);
            if (distSqrd < dist1)
            {
                dist1 = distSqrd;
                minGo1 = go;
            }
            else if (distSqrd < dist2)
            {
                dist2 = distSqrd;
                minGo2 = go;
            }
        }
        //now i have the two closest game object i will find my height

        float heightOfCurrGroundAboveSeaLevel = (minGo1.GetComponent<PoiScript>().centerAlt * dist2 + minGo2.GetComponent<PoiScript>().centerAlt * dist1) / (dist1 + dist2);
        float heightOfCurrGroundRelativeToMapCenter = heightOfCurrGroundAboveSeaLevel - defaultAlt;
        float howMuchToLiftTheMap = -heightOfCurrGroundRelativeToMapCenter;
        //now take into account the fact the the user is also moving in the AR coordinate system on the y axis
        //and that the user is holding the cam at aprx height of 1.1 meters
        //this variable will theoratically stay almost constant as the ar cam y position goes up the how much to lift goes down
        float howMuchToLiftTheMapIfPhoneIsHandHeld = arCam.transform.position.y+ howMuchToLiftTheMap - 1.1f;
        transform.position = new Vector3(transform.position.x, howMuchToLiftTheMapIfPhoneIsHandHeld, transform.position.z);

        Debug.Log("the 1st closest poi is " + minGo1.GetComponent<PoiScript>().poiName);
        Debug.Log("the 2nd closest poi is " + minGo2.GetComponent<PoiScript>().poiName);
        Debug.Log("heightOfCurrGroundAboveSeaLevel " + heightOfCurrGroundAboveSeaLevel);
        Debug.Log("heightOfCurrGroundRelativeToMapCenter " + heightOfCurrGroundRelativeToMapCenter);
        dynamicHeightText.text = "1: " + minGo1.GetComponent<PoiScript>().poiName +
                                "\n2: " + minGo2.GetComponent<PoiScript>().poiName +
                                "\nsea: " + heightOfCurrGroundAboveSeaLevel +
                                "\ncntr: " + heightOfCurrGroundRelativeToMapCenter +
                                "\ncamy: "+ arCam.transform.position.y +
                                "\nmpH: " + howMuchToLiftTheMapIfPhoneIsHandHeld;
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
