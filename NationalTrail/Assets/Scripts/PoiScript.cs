using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Poi is every item in the real world like: bridge, road, square, store, stadium
// each poi contains multiple coordinates which are connected by long rectangles
public class PoiScript : MonoBehaviour
{
    public GameObject turnPrefab;
    public GameObject connectorPrefab;
    public double centerLat { get { return _centerLat; } }
    public double centerLon { get { return _centerLon; } }

    private string TAG = "Generate PoiScript";
    private List<Tuple<double, double>> coordList;
    private List<GameObject> turnList;
    private List<GameObject> connectorList;

    private double _centerLat;
    private double _centerLon;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        coordList = new List<Tuple<double, double>>();
        turnList = new List<GameObject>();
        connectorList = new List<GameObject>();

    }

    // this method is called from within the MapScript Start()
    public void setCoordinates(List<Tuple<double, double>> coords)
    {
        coordList = coords;
        // calclate center of poi (this case it's a bridge)
        calcCenter();

        // create sphere for every turn in the poi list of coordinates
        createTurns();

        // create the lines that connect the turns
        connectTurn();
    }
    private void Start()
    {
        Debug.Log(TAG + " Start");

    }
    // create the path(cylinder) between the turns(the spheres)
    private void connectTurn()
    {
        // loop on all the turns except the last
        for(int i = 0 ; i < turnList.Count-1 ; i++)
        {
            // find the position of the connector
            double connectorX = (turnList[i].transform.position.x + turnList[i + 1].transform.position.x) / 2;
            double connectorZ = (turnList[i].transform.position.z + turnList[i + 1].transform.position.z) / 2;
            Vector3 connectorPosition = new Vector3((float)connectorX, 0, (float)connectorZ);

            
            // create the cylinder connector as child of the poi
            GameObject connector = Instantiate(connectorPrefab, connectorPosition, Quaternion.identity);
            connector.transform.parent = transform;
            connectorList.Add(connector);

            // set the rotation of the sphere
            connector.transform.LookAt(turnList[i].transform);
            connector.transform.Rotate(90, 0, 0);

            // set the scale of the connector
            float dist = getTurnsDist(i);
            connector.transform.localScale =new Vector3(0.5f, dist/2,0.5f);


        }
    }

    private float getTurnsDist(int i)
    {
        float diffX = turnList[i].transform.position.x - turnList[i + 1].transform.position.x;
        float diffZ = turnList[i].transform.position.z - turnList[i + 1].transform.position.z;
        float x_2 = Mathf.Pow(diffX, 2);
        float z_2 = Mathf.Pow(diffZ, 2);
        return Mathf.Sqrt(x_2 + z_2);
    }

    private void createTurns()
    {
        foreach (Tuple<double, double> coor in coordList)
        {
            // calculate location of every specific turn inside this poi
            Vector3 turnLocation = calcLocalPositionOfTurnInPoi(coor);

            // calculate
            GameObject sphereTurn = Instantiate(turnPrefab, turnLocation, Quaternion.identity);
            turnList.Add(sphereTurn);
            sphereTurn.transform.parent = transform;
        }
    }

    //calculate the location of the turn inside the coordinate space of the poi
    private Vector3 calcLocalPositionOfTurnInPoi(Tuple<double,double> coor)
    {
        Vector3 location;
        double z = GeoToMetersConverter.convertLatDiffToMeters(coor.Item1 - _centerLat);
        double x = GeoToMetersConverter.convertLonDiffToMeters(coor.Item2 - _centerLon, _centerLat);
        location = new Vector3((float)x, 0, -(float)z);

        return location;
    }

    private void calcCenter()
    {
        double avgLat = 0;
        double avgLon = 0;
        foreach(Tuple<double, double> coor in coordList)
        {
            avgLat += coor.Item1;
            avgLon += coor.Item2;
        }
        avgLat /= coordList.Count;
        avgLon /= coordList.Count;

        _centerLat = avgLat;
        _centerLon = avgLon;
    }

}