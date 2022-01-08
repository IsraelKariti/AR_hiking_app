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

    // Update is called once per frame
    void Update()
    {
        y += Time.deltaTime;
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Sin(y), transform.localPosition.z); ;
    }
}
