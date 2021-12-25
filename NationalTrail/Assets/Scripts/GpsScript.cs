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
    private float inAcc;

    //ANDROID GPS
    AndroidJavaObject gpsProvider;
    private long lastAndroidGPSTimeStamp = 0;

    bool isNativeAndroidGps;

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
            Debug.Log("ifandroidgps 1");
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            Debug.Log("ifandroidgps 2");
            AndroidJavaObject unityActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if(unityActivity == null)
                Debug.Log("ifandroidgps 3 null");
            else
                Debug.Log("ifandroidgps 3 ok");

            //instantiate the android plugin
            gpsProvider = new AndroidJavaObject("com.example.gpsplugin.GPSProvider", unityActivity);
            if (gpsProvider == null)
                Debug.Log("ifandroidgps gpsProvider == null");
            else
                Debug.Log("ifandroidgps gpsProvider == OK");

        }
    }

    // Update is called once per frame
    void Update()
    {
        text.text = skipSamples.ToString();
        if (isNativeAndroidGps == false)
            unityGPS();
        else
            androidGPS();
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
            inAcc = gpsProvider.Get<float>("accuracy");
            //AndroidGPSText.text = "counter: " + androidCounter;
            lastAndroidGPSTimeStamp = time;

            if (skipSamples > 0)
                skipSamples--;
            else
                OnGpsUpdated();
            //text.text = "lat: " + lat + "\nlon: " + lon + "\nacc: " + acc;
        }
    }

    private void unityGPS()
    {
        if (Input.location.status == LocationServiceStatus.Running && Input.location.lastData.timestamp > prevTimeStamp && _gpsOn && Input.location.lastData.horizontalAccuracy < 4.0f)
        {
            inLat = Input.location.lastData.latitude;
            inLon = Input.location.lastData.longitude;
            inAcc = Input.location.lastData.horizontalAccuracy;

            prevTimeStamp = Input.location.lastData.timestamp;
            if (skipSamples > 0)
                skipSamples--;
            else
                OnGpsUpdated();
            
        }
    }

    public void OnGpsUpdated()
    {
            GpsUpdatedSetMap(inLat, inLon, inAcc);
            GpsUpdatedCalcLeastSquares();

            File.AppendAllText(Application.persistentDataPath + "/coords.txt", "lat: " + inLat + " lon: " + inLon + " acc: "+inAcc+"\n");
    }

    public void EmulateGps()
    {
            GpsUpdatedSetMap(emulatorLat, emulatorLon, 0);
            GpsUpdatedCalcLeastSquares();
            File.AppendAllText(Application.persistentDataPath + "/coords.txt", "lat: " + emulatorLat + " lon: " + emulatorLon + " acc: " + inAcc + "\n");

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
