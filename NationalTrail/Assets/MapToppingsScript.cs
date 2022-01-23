using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MapToppingsScript : MonoBehaviour
{
    public Camera arCam;
    public GpsScript gpsScript;
    public GameObject poiPrefab;
    public GameObject poiConnectorPrefab;
    public GameObject waterBuryPrefab;
    public GameObject sightPrefab;
    public Text horizontalIndicationText;
    public Text verticalIndicationText;
    public double MapCenterLat { get { return mapcenterLat; } set { mapcenterLat = value; } }
    public double MapCenterLon { get { return mapcenterLon; } set { mapcenterLon = value; } }
    public float MapCenterAlt { get => mapCenterAlt; set => mapCenterAlt = value; }
    public bool IsHorizontalLocked { get => isHorizontalLocked; set => isHorizontalLocked = value; }

    private double mapcenterLat;
    private double mapcenterLon;
    private float mapCenterAlt;

    private List<string> poiFileLines;
    private List<GameObject> pois;
    private List<GameObject> poiConnectors;
    private List<GameObject> waterBuries;
    private List<GameObject> sights;

    private bool isVerticalLocked = false;
    private bool isHorizontalLocked = false;
    private int indexSight = 0;
    private bool activlyRearrangingSights = false;
    private bool isMapToppingsIntialized = false;

    private void Awake()
    {
        File.Delete(Application.persistentDataPath + "/walkedParallel.txt");
        File.Delete(Application.persistentDataPath + "/vertical_collision.txt");

        File.Delete(Application.persistentDataPath + "/toppingsLog.txt");
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "awake");
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "isMapToppingsIntialized: " + isMapToppingsIntialized + "\n");

        pois = new List<GameObject>();// point of interest(buildings, etc)
        poiConnectors = new List<GameObject>();// point of interest(buildings, etc)
        waterBuries = new List<GameObject>();
        sights = new List<GameObject>();
        poiFileLines = new List<string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "start");
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "isMapToppingsIntialized: " + isMapToppingsIntialized + "\n");

        gpsScript.GpsUpdatedSetMap += OnGpsUpdated;
    }
    public void OnGpsUpdated(double lat, double lon, float acc)
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "OnGpsUpdated\n");
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "isMapToppingsIntialized: "+ isMapToppingsIntialized+"\n");
        // this will only ever happen once
        if (!isMapToppingsIntialized)
        {
            File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "!isMapToppingsIntialized\n");

            // initialize the LAT+LON+ALT of the map to be from the first poi

            mapcenterLat = lat;
            mapcenterLon = lon;

            createMapToppings();

            InvokeRepeating("SetRearrangingSightsActive", 0, 2);
            isMapToppingsIntialized = true;
        }
    }
    private void createMapToppings()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "createMapToppings\n");
        createPois();

        // poisitioning the pois can be done only after the map center has been determind (lat lon alt)
        positionPois();
        //2) read all lines from waterburi file
        createWaterBuries();

        //3) create sights
        createSights();

        // connect all pois to a route\path
        createPoiConnectors();// this can't be called on Awake because the connectors depend on the result calculation of the position of every poi on its Start function

    }

    public void createPois()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "createPois\n");

        //1) read all lines from poi file
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/pois.txt");
        string line;

        float minDistFromCenter = 999999999;
        Vector2 center = new Vector2((float)mapcenterLat, (float)mapcenterLon);
        while ((line = reader.ReadLine()) != null)
        {
            poiFileLines.Add(line);
            GameObject go = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity, transform);

            // get all elments in line
            string[] elements = line.Split(' ');
            // set the name of the poi
            PoiScript poiScript = go.GetComponent<PoiScript>();
            poiScript.poiName = elements[0];
            poiScript.centerAlt = float.Parse(elements[elements.Length - 1]);
            // enter the coordinate of the poi (assuming there is only one poi)
            string[] latlon = elements[1].Split(',');
            float coorLat = float.Parse(latlon[0]);
            float coorLon = float.Parse(latlon[1]);
            poiScript.setCoordinates(new List<Tuple<double, double>> { new Tuple<double, double>(coorLat, coorLon) });

            // this will determine the height of map center to be the same height as the closest coordinate to map center
            float currDistToCenter = Vector2.Distance(center, new Vector2(coorLat, coorLon));
            if (currDistToCenter < minDistFromCenter)
            {
                mapCenterAlt = poiScript.centerAlt;
                minDistFromCenter = currDistToCenter;
            }

            pois.Add(go);
            File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "poi pos: "+go.transform.position+"\n");
        }
        // after i have set the toppings center alt i do the same for the parent map
        transform.parent.gameObject.GetComponent<MapScript>().MapCenterAlt = mapCenterAlt;
        reader.Close();
    }

    private void positionPois()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "positionPois\n");

        foreach (GameObject go in pois)
        {
            go.GetComponent<PoiScript>().positionPoiInMap();
        }
    }
    private void createSights()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "createSights\n");

        StreamReader reader = new StreamReader(Application.persistentDataPath + "/sights.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            GameObject go = Instantiate(sightPrefab, Vector3.zero, Quaternion.identity, transform);
            go.GetComponent<SightScript>().arCam = arCam;

            // get all elments in line
            string[] elements = line.Split(' ');

            // set the altitude of the poi
            SightScript sightScript = go.GetComponent<SightScript>();
            sightScript.Name = elements[0].Replace('_', ' ');
            sightScript.Lat = float.Parse(elements[1]);
            sightScript.Lon = float.Parse(elements[2]);
            sightScript.Alt = float.Parse(elements[3]);
            sightScript.arCam = arCam;
            go.GetComponent<TMP_Text>().text = elements[0].Replace('_', ' ');

            // position the sight on the map
            sightScript.setSightOnMap();
            sightScript.isPositioned = true;
            // add to list of sights/signs
            sights.Add(go);

        }
        reader.Close();

        // sort the list of sights by distance from camera
        sights.Sort(new CompareSightDist());
    }

    private void createWaterBuries()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "createWaterBuries\n");

        StreamReader reader = new StreamReader(Application.persistentDataPath + "/water_bury.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            GameObject go = Instantiate(waterBuryPrefab, Vector3.zero, Quaternion.identity, transform);

            // get all elments in line
            string[] elements = line.Split(' ');

            // set the altitude of the poi
            WaterBuryScript waterBuryScript = go.GetComponent<WaterBuryScript>();
            waterBuryScript.lat = float.Parse(elements[0]);
            waterBuryScript.lon = float.Parse(elements[1]);
            waterBuryScript.alt = float.Parse(elements[2]);
            // position the water bury in map
            waterBuryScript.positionWaterBuryInMap();
            waterBuryScript.isPositioned = true;
            waterBuries.Add(go);

        }
        reader.Close();
    }

    // this method is for create the route the hiker is following
    private void createPoiConnectors()
    {
        File.AppendAllText(Application.persistentDataPath + "/toppingsLog.txt", "createPoiConnectors\n");

        // assuming the pois are in their order of walking 
        for (int i = 0; i < pois.Count - 1; i++)
        {

            GameObject goTop = Instantiate(poiConnectorPrefab, Vector3.zero, Quaternion.identity, transform);
            // position the connectors
            PoiConnectorScript poiConnectorScript = goTop.GetComponent<PoiConnectorScript>();
            poiConnectorScript.ArCam = arCam;
            poiConnectorScript.positionInMap(pois[i], pois[i + 1]);

            poiConnectors.Add(goTop);

        }
    }
    // this function induce the signs to be rearrange so they won't hide each other
    private void SetRearrangingSightsActive()
    {
        sights.Sort(new CompareSightDist());
        indexSight = 0;
        activlyRearrangingSights = true;
    }
    private void Update()
    {
        // if the topping hasn't been set yet than don't do anything
        if (isMapToppingsIntialized)
        {
            // at the begining the map doesn't know its height, so we estimate from the 2 closest points,
            // this should be stopped when a collider is hit, cause than the height is known (colliders are at a known height)
            if (!isVerticalLocked)
                evaluateTemporaryInitialMapHeight();

            // this is called every couple of seconds
            // move each sight on different frame,
            // so that the affects on one sight can be taken into account in the second sight
            // this call is making the sign to not hide each other
            if (activlyRearrangingSights && indexSight < sights.Count)
            {
                sights[indexSight++].GetComponent<SightScript>().Reheight();
                if(indexSight == sights.Count)
                    File.AppendAllText(Application.persistentDataPath + "/reheight.txt", "\n\n\n\n\n");

            }
            else
            {

                activlyRearrangingSights = false;

            }
        }
    }

    public void evaluateTemporaryInitialMapHeight()
    {
        if (gpsScript.sampleCountForInitialMapPosition > 1)
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
                float heightOfCurrGroundRelativeToMapCenter = heightOfCurrGroundAboveSeaLevel - MapCenterAlt;
                float howMuchToLiftTheMap = -heightOfCurrGroundRelativeToMapCenter;
                //now take into account the fact the the user is also moving in the AR coordinate system on the y axis
                //and that the user is holding the cam at aprx height of 1.1 meters
                //this variable will theoratically stay almost constant as the ar cam y position goes up the how much to lift goes down
                float howMuchToLiftTheMapIfPhoneIsHandHeld = arCam.transform.position.y + howMuchToLiftTheMap - 1.1f;
                transform.position = new Vector3(transform.position.x, howMuchToLiftTheMapIfPhoneIsHandHeld, transform.position.z);
            }
        }
    }
    // this function is suppose to replace the initial fluctuating height of the map that is constantly changeing every update call
    // this will occur when the user has walked parallel to a connector and this will determine the shift (+-3meters)
    // and after the shift has been determined than it will be time to enable the height locking with the poi collider
    // this function can't determine the height as this path is 1 meters away from the poi, so the height is not determined as good as in the poi
    public void OnUserWalkedParallelToConnector(Vector2 designatedLocalShiftInMapXZ)
    {
        File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "\n\n\n\n\n"+ DateTime.Now+ "\n");
        File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", ""+ DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");
        File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "parallel\n");

        // this only occur if the map is positioned already geographcally stable enough
        // otherwise the LS script won't enable the detection game object
        {
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "gps samples: "+ gpsScript.sampleCountForInitialMapPosition + "\n");

            //since the toppings can only move in a radius of 3 meters from the gps induced map
            // we have to make sure that the toppings will be in this bound
            Vector3 globalToppingsPos = transform.position;
            Vector3 designatedLocalShiftInMapXYZ = new Vector3(designatedLocalShiftInMapXZ.x, 0, designatedLocalShiftInMapXZ.y);
            Vector3 designatedLocalToppingsPosition = transform.localPosition + designatedLocalShiftInMapXYZ;
            Vector2 designatedLocalToppingsPositionXZ = new Vector2(designatedLocalToppingsPosition.x, designatedLocalToppingsPosition.z);

            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "isHorizontalLocked: " + isHorizontalLocked + "\n");
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "globalToppingsPos: "+ globalToppingsPos+"\n");
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "localToppingsPos: "+ transform.localPosition+"\n");
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "shiftXYZ: " + designatedLocalShiftInMapXYZ + "\n");
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "designatedLocalToppingsPosition: " + designatedLocalToppingsPosition + "\n");
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "designatedLocalToppingsPositionXZ.sqrMagnitude: " + designatedLocalToppingsPositionXZ.sqrMagnitude + "\n");
            File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "Values.GPS_ERROR_RADIUS_SQRD: " + Values.GPS_ERROR_RADIUS_SQRD + "\n");

            // check if the toppings will move horizontally to a place within the gps error radius
            if ( designatedLocalToppingsPositionXZ.sqrMagnitude < Values.GPS_ERROR_RADIUS_SQRD &&
                designatedLocalShiftInMapXZ.sqrMagnitude > Values.MIN_THRESHOLD_SHIFT_SQRD)
            {
                // this will cancel the initial dynamic valuation of height
                transform.localPosition += designatedLocalShiftInMapXYZ;// the shift is 2 dimension XZ, so the y value of the vector is the z global axis
                isHorizontalLocked = true;
                horizontalIndicationText.text = "H";
                // move the map toppings with respect to the base map
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "TOPPINGS SHIFTED!" + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "AFTER SHIFT toppings global pos: " + transform.position + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "AFTER SHIFT toppings local pos: " + transform.localPosition + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "AFTER SHIFT map->toppings: " + Vector3.Distance(transform.position, transform.parent.position) + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "AFTER SHIFT isHorizontalLocked: " + isHorizontalLocked + "\n");

            }
            // only if the user is far away from the map (ex. on the other side of valley then reset)
            else if(designatedLocalToppingsPositionXZ.sqrMagnitude > Values.GPS_ERROR_RADIUS_SQRD)// if the shift of the toppings is not subltle anymore and the designated position is out of the 4 meter radius
            {
                transform.localPosition = Vector3.zero;
                isHorizontalLocked = false;
                isVerticalLocked = false;
                horizontalIndicationText.text = "";
                verticalIndicationText.text = "";

                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "TOPPINGS RESET!" + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "TOPPINGS RESET toppings global pos: " + transform.position + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "TOPPINGS RESET toppings local pos: " + transform.localPosition + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "TOPPINGS RESET isHorizontalLocked: " + isHorizontalLocked + "\n");
                File.AppendAllText(Application.persistentDataPath + "/walkedParallel.txt", "TOPPINGS RESET isVerticalLocked: " + isVerticalLocked + "\n");
            }
        }
    }
    // this is called when the user (holding the phone and camera) is above the poi
    public void OnCamTriggeredPoiEnter(Collider turn)
    {
        File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "\n\n\n\n\nentered vertical collision" + "\n");
        File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "isHorizontalLocked: "+ isHorizontalLocked + "\n");

        // if the toppings are aligned with the actual physical trail than the pois are located on the actual physical coords
        if (isHorizontalLocked)
        {
            // If i could count on the AR of the phone to give accurate y axis changes during a long period of time than this next lines are redundant
            // but just to make sure the height of the map is correct i will run this code every time the user is passing a poi
            // set the height of the map to a fixed height based on the height of the poi (assuming user is walking on ground + holding the phone at 1.1m height above ground)
            float poiLocalYInMap = turn.transform.parent.localPosition.y;
            File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "poiLocalYInMap: " + poiLocalYInMap + "\n");
            float camHeightInAR = arCam.transform.position.y;
            File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "camHeightInAR: " + camHeightInAR + "\n");
            float userFeetHeightInAR = camHeightInAR - 1.1f;
            File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "userFeetHeightInAR: " + userFeetHeightInAR + "\n");
            //how much to lift the map:
            float liftTheMap = userFeetHeightInAR - poiLocalYInMap;
            File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "liftTheMap: " + liftTheMap + "\n");
            // change map height
            transform.position = new Vector3(transform.position.x, liftTheMap, transform.position.z);
            isVerticalLocked = true;
            File.AppendAllText(Application.persistentDataPath + "/vertical_collision.txt", "isVerticalLocked: " + isVerticalLocked + "\n");
            verticalIndicationText.text = "V";

        }

        // this will invoke the dependency of the height map from the y value of the camera
        // after this line the height of the map will be lock to the height of the first
        //if (gpsScript.sampleCountForInitialMapPosition > 5)// this should only occur if the map is positioned already geographcally
        //{
        //    isHeightLocked = true;
        //    // If i could count on the AR of the phone to give accurate y axis changes during a long period of time than this next lines are redundant
        //    // but just to make sure the height of the map is correct i will run this code every time the user is passing a poi
        //    // set the height of the map to a fixed height based on the height of the poi (assuming user is walking on ground + holding the phone at 1.1m height above ground)
        //    float poiLocalYInMap = turn.gameObject.transform.parent.localPosition.y;
        //    float camHeightInAR = arCam.transform.position.y;
        //    float userFeetHeightInAR = camHeightInAR - 1.1f;
        //    //how much to lift the map:
        //    float liftTheMap = userFeetHeightInAR - poiLocalYInMap;
        //    // change map height
        //    transform.position = new Vector3(transform.position.x, liftTheMap, transform.position.z);
        //}

    }

    
}
