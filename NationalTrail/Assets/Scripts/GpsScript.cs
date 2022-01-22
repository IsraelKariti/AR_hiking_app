using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class GpsScript : MonoBehaviour
{
    public Text text;
    public Camera arCam;
    private string TAG = "GpsScript";
    private double prevTimeStamp;
    private int _skipSamples = Values.SKIP_SAMPLES;
    private float _avgLat;
    private float _avgLon;
    private bool _gpsOn;
    private int _sampleCountForInitialMapPosition=0;
    public int sampleCountForInitialMapPosition { get { return _sampleCountForInitialMapPosition; } }
    public float avgLat { get { return _avgLat; } }
    public float avgLon { get { return _avgLon; } }
    public int skipSamples { get { return _skipSamples; } }
    // listener for the map+ground
    public delegate void GpsUpdatedSetSampleEventHandler(double lat, double lon, float acc);
    public event GpsUpdatedSetSampleEventHandler GpsUpdatedSetMap;
    public delegate void GpsUpdatedLeastSquaresEventHandler();
    public event GpsUpdatedLeastSquaresEventHandler GpsUpdatedCalcLeastSquares;
    public bool gpsOn { set { _gpsOn = value; } }

    private double inLat;// the input location lat 
    private double inLon;
    private float inHorizontalAcc;
    private double inAlt;
    private float inAltAcc;
    private float emuLon = 34f;
    private void Awake()
    {
        if (!Input.location.isEnabledByUser) //FIRST IM CHACKING FOR PERMISSION IF "true" IT MEANS USER GAVED PERMISSION FOR USING LOCATION INFORMATION
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        File.Delete(Application.persistentDataPath + "/coordinates.txt");
        File.Delete(Application.persistentDataPath + "/gps.txt");
        File.AppendAllText(Application.persistentDataPath + "/coordinates.txt", "lat,lon\n");

        // unity gps is superrior to native android gps. when i walk in a straight line everything is fine,
        // but when i aim the phone sideways to look at a building the android gps throws the results around.
        // it doesn't deal well with moving the phone all over
        // this effect is much less notable in unity gps
    }
    // Start is called before the first frame update
    void Start()
    {
        _gpsOn = true;
        Input.location.Start(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
       
        text.text = _skipSamples.ToString();
        unityGPS();
        //EmulateGPS();
        File.AppendAllText(Application.persistentDataPath + "/gps.txt", "update\n");

    }

    private void unityGPS()
    {
        File.AppendAllText(Application.persistentDataPath + "/gps.txt", "unityGPS\n");

        if (Input.location.status == LocationServiceStatus.Running && Input.location.lastData.timestamp > prevTimeStamp && _gpsOn && Input.location.lastData.horizontalAccuracy < 8.0f)
        {
            inLat = Input.location.lastData.latitude;
            inLon = Input.location.lastData.longitude;
            inHorizontalAcc = Input.location.lastData.horizontalAccuracy;
            inAlt = Input.location.lastData.altitude;
            inAltAcc = Input.location.lastData.verticalAccuracy;

            prevTimeStamp = Input.location.lastData.timestamp;

            if (_skipSamples > 0)
                _skipSamples--;
            else
            {
                _sampleCountForInitialMapPosition++;
                
                OnGpsUpdated();
            }
        }
    }

    public void OnGpsUpdated()
    {
        File.AppendAllText(Application.persistentDataPath + "/gps.txt", "OnGpsUpdated\n");

        File.AppendAllText(Application.persistentDataPath + "/coordinates.txt", "" + inLat + "," + inLon + "\n");
        GpsUpdatedSetMap(inLat, inLon, inHorizontalAcc);
        GpsUpdatedCalcLeastSquares();
    }

    //public void EmulateGPS()
    //{
    //    _sampleCountForInitialMapPosition++;
    //    GpsUpdatedSetMap(31f, emuLon, 3);
    //    GpsUpdatedCalcLeastSquares();
    //    emuLon -= 0.0001f;
    //}

    public void switchGPS(bool val)
    {
        gpsOn = val;
    }
}
