using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBuryScript : MonoBehaviour
{

    private double _lat;
    private double _lon;
    private float _alt;
    public double lat { get { return _lat; } set { _lat = value; } }
    public double lon { get { return _lon; } set { _lon = value; } }
    public float alt { get { return _alt; } set { _alt = value; } }
    private float y = 0;
    public bool isPositioned = false;

    public void positionWaterBuryInMap()
    {
        MapToppingsScript mapToppingsScript = transform.parent.GetComponent<MapToppingsScript>();
        // calcula the position of the center of the poi
        double zMeters = GeoToMetersConverter.convertLatDiffToMeters(mapToppingsScript.MapCenterLat - lat);
        double xMeters = GeoToMetersConverter.convertLonDiffToMeters(mapToppingsScript.MapCenterLon - lon, mapToppingsScript.MapCenterLat);
        double yMeters = alt - mapToppingsScript.MapCenterAlt;
        // position the water bury in the map
        transform.localPosition = new Vector3(-(float)xMeters, (float)yMeters, -(float)zMeters);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPositioned)
        {
            y += Time.deltaTime;
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Sin(y), transform.localPosition.z);
        }
         
    }
}
