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
    //private int limitSamples = 10;
    private int skipSamples = 10;
    private float _avgLat;
    private float _avgLon;
    private bool _gpsOn;
    public float avgLat { get { return _avgLat; } }
    public float avgLon { get { return _avgLon; } }
    
    // listener for the map+ground
    public delegate void GpsUpdatedSetSampleEventHandler(double lat, double lon, float acc);
    public event GpsUpdatedSetSampleEventHandler GpsUpdatedSetMap;
    public delegate void GpsUpdatedLeastSquaresEventHandler();
    public event GpsUpdatedLeastSquaresEventHandler GpsUpdatedCalcLeastSquares;
    public bool gpsOn { set { _gpsOn = value; } }
    private double emulatorLat = 31.26255f;
    private double emulatorLon = 34.79350f;
    private double inLat;// the input location lat 
    private double inLon;
    private float inHorizontalAcc;
    private double inAlt;
    private float inAltAcc;

    //ANDROID GPS
    AndroidJavaObject gpsProvider;
    private long lastAndroidGPSTimeStamp = 0;

    bool isNativeAndroidGps;
    string pre;
    private void Awake()
    {
        if (!Input.location.isEnabledByUser) //FIRST IM CHACKING FOR PERMISSION IF "true" IT MEANS USER GAVED PERMISSION FOR USING LOCATION INFORMATION
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        isNativeAndroidGps = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        _gpsOn = true;
        File.Delete(Application.persistentDataPath + "/coords.txt");

        if (isNativeAndroidGps == false)
        {
            Input.location.Start(0, 1);
        }
        else
        {
            //=======================GPS ANDROID==========================
            // create the current UNITY activity
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");

            //instantiate the android plugin
            gpsProvider = new AndroidJavaObject("com.example.gpsplugin.GPSProvider", unityActivity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        text.text = skipSamples.ToString();
        if (isNativeAndroidGps == false)
        {
            pre = "U: ";
            unityGPS();
        }
        else
        {
            pre = "A: ";
            androidGPS();
        }
    }

    private void androidGPS()
    {
        long time = gpsProvider.Get<long>("time");
        //bool availability = gpsProvider.Get<bool>("availability");

        if (time > lastAndroidGPSTimeStamp&& _gpsOn)
        {
            // calculate new lat-lon for the origin 
            inLat = gpsProvider.Get<double>("lat");

            inLon = gpsProvider.Get<double>("lon");

            inHorizontalAcc = gpsProvider.Get<float>("accuracy");
            inAlt = gpsProvider.Get<double>("alt");
            inAltAcc = gpsProvider.Get<float>("altAcc");

            lastAndroidGPSTimeStamp = time;

            if (skipSamples > 0)
                skipSamples--;
            else
                OnGpsUpdated();
        }
    }

    private void unityGPS()
    {
        if (Input.location.status == LocationServiceStatus.Running && Input.location.lastData.timestamp > prevTimeStamp && _gpsOn && Input.location.lastData.horizontalAccuracy < 4.0f)
        {
            inLat = Input.location.lastData.latitude;
            inLon = Input.location.lastData.longitude;
            inHorizontalAcc = Input.location.lastData.horizontalAccuracy;
            inAlt = Input.location.lastData.altitude;
            inAltAcc = Input.location.lastData.verticalAccuracy;

            prevTimeStamp = Input.location.lastData.timestamp;

            if (skipSamples > 0)
                skipSamples--;
            else
                OnGpsUpdated();
            
        }
    }

    public void OnGpsUpdated()
    {
        GpsUpdatedSetMap(inLat, inLon, inHorizontalAcc);
        GpsUpdatedCalcLeastSquares();
        float eleveationFromFloor = getElevationFromFloor(); 
        File.AppendAllText(Application.persistentDataPath + "/coords.txt", pre+":lat:" + inLat + ":lon:" + inLon + ":acc:"+inHorizontalAcc+":alt:"+inAlt+":altAcc:"+inAltAcc+":elev:"+eleveationFromFloor.ToString("0.0")+"\n");
    }
    private float getElevationFromFloor()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -Vector3.up, out hit))
        {
            return hit.distance;
        }
        else
            return 1.1f;// the default height of smartphone above ground while held by user
    }
    public void EmulateGps()
    {
            GpsUpdatedSetMap(emulatorLat, emulatorLon, 0);
            GpsUpdatedCalcLeastSquares();
            File.AppendAllText(Application.persistentDataPath + "/coords.txt", pre+ "lat: " + emulatorLat + " lon: " + emulatorLon + " acc: " + inHorizontalAcc + " alt: " + inAlt + " altAcc: " + inAltAcc + "\n");

        emulatorLon -= 0.00003f;
    }

    public void switchGPS(bool val)
    {
        gpsOn = val;
    }
    public void toggleGpsAndroidSource(bool val)
    {
        isNativeAndroidGps = val;
    }
}
