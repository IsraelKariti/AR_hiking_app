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
    public GameObject poiPrefab;
    public Camera arCam;
    public GameObject poiConnectorPrefab;
    public GameObject waterBuryPrefab;
    public GameObject sightPrefab;
    public double centerLat { get { return _centerLat; } set { _centerLat = value; } }
    public double centerLon { get { return _centerLon; } set { _centerLon = value; } }
    public List<GameObject> mapSamples { get { return _samples; } }
   
    private string TAG = "Generate MapScript";
   
    private List<GameObject> _samples;
    // all the way to Oren center

    private double _centerLat;
    private double _centerLon;
    private float defaultAlt = 290f;
    //private double widthMeters;//the east<-->west in meters 
    //private double lengthMeters;// the north<-->south in meters
    private List<string> poiFileLines;
    private List<GameObject> pois;
    private List<GameObject> poiConnectors;
    private List<GameObject> waterBuries;
    private List<GameObject> sights;

    private bool initCenter;
    private bool heightInitialEstimation = true;
    private void Awake()
    {
        initCenter = false;
        _samples = new List<GameObject>();// gps samples
        pois = new List<GameObject>();// point of interest(buildings, etc)
        poiConnectors = new List<GameObject>();// point of interest(buildings, etc)
        waterBuries = new List<GameObject>();
        sights = new List<GameObject>();
        poiFileLines = new List<string>();

        //TODO:
        //1) read all lines from poi file
        StreamReader reader = new StreamReader(Application.persistentDataPath+"/pois.txt");
        string line;
        while((line = reader.ReadLine())!=null)
        {
            poiFileLines.Add(line);
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
        //2) read all lines from waterburi file
        createWaterBuries();

        //3) create sights
        createSights();
    }

    private void createSights()
    {
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/sights.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            GameObject go = Instantiate(sightPrefab, Vector3.zero, Quaternion.identity, transform);
            go.GetComponent<SightScript>().arCam = arCam;
            sights.Add(go);

            // get all elments in line
            string[] elements = line.Split(' ');

            // set the altitude of the poi
            
            go.GetComponent<SightScript>().name = elements[0].Replace('_',' ');
            go.GetComponent<SightScript>().lat = float.Parse(elements[1]);
            go.GetComponent<SightScript>().lon = float.Parse(elements[2]);
            go.GetComponent<SightScript>().alt = float.Parse(elements[3]);
            go.GetComponent<TMP_Text>().text = elements[0].Replace('_', ' ');


        }
        reader.Close();
    }

    private void createWaterBuries()
    {
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/water_bury.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            GameObject go = Instantiate(waterBuryPrefab, Vector3.zero, Quaternion.identity, transform);
            waterBuries.Add(go);

            // get all elments in line
            string[] elements = line.Split(' ');

            // set the altitude of the poi
            go.GetComponent<WaterBuryScript>().lat = float.Parse(elements[0]);
            go.GetComponent<WaterBuryScript>().lon = float.Parse(elements[1]);
            go.GetComponent<WaterBuryScript>().alt = float.Parse(elements[2]);

        }
        reader.Close();
    }


    // Start is called before the first frame update
    void Start()
    {
        

        //TODO:
        //1) for each line in the file create list of 4 tuples and call each poi setCoordinates
        for(int i = 0;i < pois.Count;i++)
        {
            // get all elments in line
            string[] elements = poiFileLines[i].Split(' ');
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

        // add the water buries to the map
        positionWaterBuries();

        // add the sights (hospital, universicty, etc) to the map
        positionSights();

        gpsScript.GpsUpdatedSetMap += OnGpsUpdated;
    }

    private void positionSights()
    {
        foreach (GameObject go in sights)
        {
            // get coordinates of bury
            double lat = go.GetComponent<SightScript>().lat;
            double lon = go.GetComponent<SightScript>().lon;
            double alt = go.GetComponent<SightScript>().alt;
            // calcula the position of the center of the poi
            double zMeters = GeoToMetersConverter.convertLatDiffToMeters(_centerLat - lat);
            double xMeters = GeoToMetersConverter.convertLonDiffToMeters(_centerLon - lon, _centerLat);
            double yMeters = alt - defaultAlt;
            // position the sight in the map
            go.transform.localPosition = new Vector3(-(float)xMeters, (float)yMeters, -(float)zMeters);
        }
    }

    private void positionWaterBuries()
    {
        foreach(GameObject go in waterBuries)
        {
            // get coordinates of bury
            double lat = go.GetComponent<WaterBuryScript>().lat;
            double lon = go.GetComponent<WaterBuryScript>().lon;
            double alt = go.GetComponent<WaterBuryScript>().alt;
            // calcula the position of the center of the poi
            double zMeters = GeoToMetersConverter.convertLatDiffToMeters(_centerLat - lat);
            double xMeters = GeoToMetersConverter.convertLonDiffToMeters(_centerLon - lon, _centerLat);
            double yMeters = alt - defaultAlt;
            // position the water bury in the map
            go.transform.localPosition = new Vector3(-(float)xMeters, (float)yMeters, -(float)zMeters);
        }
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
            // get the location of the connector between pois
            float x = (pois[i].transform.localPosition.x + pois[i + 1].transform.localPosition.x) / 2;
            float y = (pois[i].transform.localPosition.y + pois[i + 1].transform.localPosition.y) / 2;
            float z = (pois[i].transform.localPosition.z + pois[i + 1].transform.localPosition.z) / 2;
            GameObject goTop = Instantiate(poiConnectorPrefab, Vector3.zero, Quaternion.identity, transform);
            //GameObject goBottom = Instantiate(poiConnectorPrefab, Vector3.zero, Quaternion.identity, goTop.transform);
            poiConnectors.Add(goTop);
            // locate the connector between the two pois
            goTop.transform.localPosition = new Vector3(x, y, z);
            Debug.Log("pois go.transform.localPosition " + goTop.transform.localPosition.ToString());

            //go.transform.localScale = new Vector3(1, Vector3.Distance(pois[i].transform.localPosition, pois[i + 1].transform.localPosition), 1);
            //goTop.GetComponent<SpriteRenderer>().size = new Vector2(1, Vector3.Distance(pois[i].transform.localPosition, pois[i + 1].transform.localPosition));
            goTop.GetComponentInChildren<SpriteRenderer>().size = new Vector2(1, Vector3.Distance(pois[i].transform.localPosition, pois[i + 1].transform.localPosition));
            Debug.Log("pois go.transform.localScale " + goTop.transform.localScale.ToString());

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

            goTop.transform.localRotation = Quaternion.Euler(rotX, rotY, 0);
        }
    }
    public void rotateConnector(float f)
    {
        //poiConnectors[0].GetComponentsInChildren<Transform>()[1].localRotation = Quaternion.Euler(0, f, 0);
        poiConnectors[0].transform.localRotation = Quaternion.Euler(0, f, 0);
    }
    private void Update()
    {
        setMapHeight();
        adjustConnectors();
    }

    private void adjustConnectors()
    {
        setConnectorsRotation();
        setConnectorsColor();
    }

    private void setConnectorsColor()
    {
        // loop on all connector
        foreach (GameObject go in poiConnectors)
        {
            // get the connector
            GameObject child = go.transform.GetChild(0).gameObject;
            
            // get the world position of the camera
            Vector3 camPos = arCam.transform.position;

            // check dist cam-connector
            float dist = Vector3.Distance(camPos, child.transform.position);

            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            Material mat = renderer.material;
            Color c = mat.GetColor("_Color");
            c.a = 0.2f+dist / 100.0f;
            mat.SetColor("_Color", c);

        }
    }

    public void setConnectorsRotation()
    {
        // loop on all planes
        foreach(GameObject go in poiConnectors)
        {
            // get the child
            GameObject child = go.transform.GetChild(0).gameObject;
            // get the world position of the camera
            Vector3 camPos = arCam.transform.position;

            // get the position of the camera in the child frame of reference
            Vector3 camInConnector = go.transform.InverseTransformPoint(camPos);
            // calc the angle of rotation between the connector and the camera
            float rotY = Mathf.Atan2(-camInConnector.x, -camInConnector.z)*Mathf.Rad2Deg;

            int HorizonHigh;
            int HorizonLow;
            int responsiveRadius = 20;
            // make the rotation threshold less responsive for closer connectors
            // because they take so much screen space the you can notice them easily even from low angle
            if(Vector3.Distance(child.transform.position, arCam.transform.position) < responsiveRadius)
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
            if(rotY <HorizonHigh && rotY > -HorizonHigh)
            {
                rotYPhase = 0;
            } else if(rotY>HorizonHigh && rotY < HorizonLow)
            {
                rotYPhase = 90;
            }else if(rotY> HorizonLow || rotY < -HorizonLow)
            {
                rotYPhase = 180;
            }else if(rotY<-HorizonHigh && rotY > -HorizonLow)
            {
                rotYPhase = -90;
            }
            child.transform.localRotation = Quaternion.Euler(0,rotYPhase , 0);
        }
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
        foreach(TextMeshPro tmp in sample.GetComponentsInChildren<TextMeshPro>())
        {
            tmp.text = acc.ToString("0.0");
        }
        //sample.GetComponentsInChildren<TextMeshPro>()[0].text = acc.ToString("0.0");
        //sample.GetComponentsInChildren<TextMeshPro>()[1].text = acc.ToString("0.0");
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
    public void setMapHeight()
    {
        if (heightInitialEstimation&& gpsScript.sampleCountForInitialMapPosition > 3)
        {
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
            // THIS ENTIRE ALGORITHM IS FLAWED!!! THE GROUND UP AND DOWN BETWEEN POIS IS NOT LINEAR,
            // SOMETIMES HIGHER (OR LOWER) THAN CALCULATED HEIGHT
            // THIS IS CAUSING THE MAP TO RISE ABOVE GROUND IN AN UNSEXY FASHION
            // The solution is to reelevate the map every time i am on a poi (because pois have known locations)
            {
                float heightOfCurrGroundAboveSeaLevel = (minGo1.GetComponent<PoiScript>().centerAlt * dist2 + minGo2.GetComponent<PoiScript>().centerAlt * dist1) / (dist1 + dist2);
                float heightOfCurrGroundRelativeToMapCenter = heightOfCurrGroundAboveSeaLevel - defaultAlt;
                float howMuchToLiftTheMap = -heightOfCurrGroundRelativeToMapCenter;
                //now take into account the fact the the user is also moving in the AR coordinate system on the y axis
                //and that the user is holding the cam at aprx height of 1.1 meters
                //this variable will theoratically stay almost constant as the ar cam y position goes up the how much to lift goes down
                float howMuchToLiftTheMapIfPhoneIsHandHeld = arCam.transform.position.y + howMuchToLiftTheMap - 1.1f;
                transform.position = new Vector3(transform.position.x, howMuchToLiftTheMapIfPhoneIsHandHeld, transform.position.z);
            }
        }
    }

    // this is called when the user (holding the phone and camera) is above the poi
    public void OnCamTriggeredPoiEnter(Collider turn)
    {
        // this will invoke the dependency of the height map from the y value of the camera
        // after this line the height of the map will be lock to the height of the first
        if (gpsScript.sampleCountForInitialMapPosition > 3)// this should only occur if the map is positioned already geographcally
        {
            heightInitialEstimation = false;

            // If i could count on the AR of the phone to give accurate y axis changes during a long period of time than this next lines are redundant
            // but just to make sure the height of the map is correct i will run this code every time the user is passing a poi
            // set the height of the map to a fixed height based on the height of the poi (assuming user is walking on ground + holding the phone at 1.1m height above ground)
            float poiLocalYInMap = turn.gameObject.transform.parent.localPosition.y;
            float camHeightInAR = arCam.transform.position.y;
            float userFeetHeightInAR = camHeightInAR - 1.1f;
            //how much to lift the map:
            float liftTheMap = userFeetHeightInAR - poiLocalYInMap;
            // change map height
            transform.position = new Vector3(transform.position.x, liftTheMap, transform.position.z);
        }
    }
}
