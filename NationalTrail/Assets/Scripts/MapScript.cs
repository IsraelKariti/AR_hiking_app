using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
// this class represents a constant rectangular area (Tile) 
// pois in the area will be the children of this game object
public class MapScript : MonoBehaviour
{
    public double centerLat { get { return _centerLat; } set { _centerLat = value; } }
    public double centerLon { get { return _centerLon; } set { _centerLon = value; } }
    public GpsScript gpsScript;
    public GameObject gpsSamplePrefab;
    public GameObject poiPrefab;

    public List<GameObject> mapSamples { get { return _samples; } }
   


    private string TAG = "Generate MapScript";
   
    private List<GameObject> _samples;
    // all the way to Oren center

    private readonly double upRightCornerLat = 31.26269f;
    private readonly double upRightCornerLon = 34.79464f;
    private readonly double downLeftCornerLat = 31.26181f;
    private readonly double downLeftCornerLon = 34.79246f;

    private double _centerLat;
    private double _centerLon;
    private double widthMeters;//the east<-->west in meters 
    private double lengthMeters;// the north<-->south in meters

    private GameObject poiGameObject_3;
    private GameObject poiGameObject_zooStore;
    private GameObject poiGameObject_10;
    private GameObject poiGameObject_10SideWalk;
    private void Awake()
    {

        _samples = new List<GameObject>();

        //create poi for Avraham Avinu 3
        poiGameObject_3 = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity, transform);
        poiGameObject_10 = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity, transform);
        poiGameObject_10SideWalk = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity, transform);
        poiGameObject_zooStore = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity, transform);

    }

    // Start is called before the first frame update
    void Start()
    {
        // calculate the center of the tile
        _centerLat = (downLeftCornerLat + upRightCornerLat)/ 2;
        _centerLon = (downLeftCornerLon + upRightCornerLon)/ 2;

        // calculate the physical dimensions of the tile
        widthMeters = GeoToMetersConverter.convertLatDiffToMeters(Math.Abs(downLeftCornerLat - upRightCornerLat));
        lengthMeters = GeoToMetersConverter.convertLonDiffToMeters(Math.Abs(downLeftCornerLon - upRightCornerLon), centerLat);

        // set the size and position of the borders
        //setBorders();


        // initialize pois
        poiGameObject_3.GetComponent<PoiScript>().setCoordinates(new List<Tuple<double, double>>() {    new Tuple<double, double>(31.2623015, 34.7933891), // avraham avinu 3
                                                                                                new Tuple<double, double>(31.2623043, 34.7938659),
                                                                                                new Tuple<double, double>(31.2622231, 34.7938669),
                                                                                                new Tuple<double, double>(31.2622228, 34.7933916)});
        poiGameObject_zooStore.GetComponent<PoiScript>().setCoordinates(new List<Tuple<double, double>>() {    new Tuple<double, double>(31.262304, 34.7938649), // zoo store
                                                                                                new Tuple<double, double>(31.2622235, 34.7938673),
                                                                                                new Tuple<double, double>(31.2622233, 34.7938027),
                                                                                                new Tuple<double, double>(31.2623038, 34.7937982)});
        // avraham avinu 10
        poiGameObject_10.GetComponent<PoiScript>().setCoordinates(new List<Tuple<double, double>>() {    new Tuple<double, double>(31.2635667, 34.7939018), // NE
                                                                                                        new Tuple<double, double>(31.2630284, 34.7939135),//SE
                                                                                                        new Tuple<double, double>(31.2630274, 34.793802),//SW
                                                                                                        new Tuple<double, double>(31.2635661, 34.793791)});//NW
        poiGameObject_10SideWalk.GetComponent<PoiScript>().setCoordinates(new List<Tuple<double, double>>() {       new Tuple<double, double>(31.263405, 34.7937523), // NE
                                                                                                                    new Tuple<double, double>(31.2630667, 34.7937593),//SE
                                                                                                                    new Tuple<double, double>(31.263066, 34.7937302),//SW
                                                                                                                    new Tuple<double, double>(31.2634055, 34.7937232)});//NW

        // add pois to map
        addPois();

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
    
    //private void setBorders()
    //{
    //    // resize the edges of the tile
    //    transform.GetChild(0).localScale = new Vector3((float)widthMeters, 1, 1);// north
    //    transform.GetChild(1).localScale = new Vector3((float)widthMeters, 1, 1);// south
    //    transform.GetChild(2).localScale = new Vector3(1, 1, (float)lengthMeters);// east
    //    transform.GetChild(3).localScale = new Vector3(1, 1, (float)lengthMeters);// west

    //    // position the edges of the tile
    //    transform.GetChild(0).position = new Vector3(0, 0, (float)lengthMeters / 2);// north
    //    transform.GetChild(1).position = new Vector3(0, 0, -(float)lengthMeters / 2);// south
    //    transform.GetChild(2).position = new Vector3((float)widthMeters / 2, 0, 0);// east
    //    transform.GetChild(3).position = new Vector3(-(float)widthMeters / 2, 0, 0);// west    }
    //}

    public void OnGpsUpdated(double lat, double lon, float acc)
    {

        // calculate the x-z of the sample
        Vector3 samplePosition;


        double z = GeoToMetersConverter.convertLatDiffToMeters(_centerLat - lat);
        double x = GeoToMetersConverter.convertLonDiffToMeters(_centerLon - lon , _centerLat);

        samplePosition = new Vector3(-(float)x, 0, -(float)z);
        // calculate the y of the sample
        
        // create the sample 3D text
        GameObject sample = Instantiate(gpsSamplePrefab, Vector3.zero, Quaternion.identity, transform);
        sample.transform.localPosition = samplePosition;
        sample.GetComponentsInChildren<TextMeshPro>()[0].text = acc.ToString("0.0");
        sample.GetComponentsInChildren<TextMeshPro>()[1].text = acc.ToString("0.0");
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
    public void setMapHeight(float h)
    {
        transform.position = new Vector3(transform.position.x, h, transform.position.z);
    }
}
