using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;

public class GpsScript : MonoBehaviour
{
    private string TAG = "GpsScript";
    private double prevTimeStamp;
    //private int limitSamples = 10;
    private int skipSamples = 10;
    private float _avgLat;
    private float _avgLon;
    private List<float> latSamples;
    private List<float> lonSamples;
    private bool _gpsOn;
    public float avgLat { get { return _avgLat; } }
    public float avgLon { get { return _avgLon; } }
    
    public delegate void GpsUpdatedEventHandler(float lat, float lon);
    public event GpsUpdatedEventHandler GpsUpdated;
    public delegate void GpsUpdatedPhase2EventHandler();
    public event GpsUpdatedPhase2EventHandler GpsUpdatedPhase2;
    public bool gpsOn { set { _gpsOn = value; } }
    private float emulatorLat = 31.26255f;
    private float emulatorLon = 34.79350f;
    private void Awake()
    {
        if (!Input.location.isEnabledByUser) //FIRST IM CHACKING FOR PERMISSION IF "true" IT MEANS USER GAVED PERMISSION FOR USING LOCATION INFORMATION
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Input.location.Start(0,1);
        latSamples = new List<float>();
        lonSamples = new List<float>();
        _gpsOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.location.status == LocationServiceStatus.Running && Input.location.lastData.timestamp > prevTimeStamp && _gpsOn)
        {
            prevTimeStamp = Input.location.lastData.timestamp;
            Debug.Log(TAG + " lat " + Input.location.lastData.latitude+" lon "+ Input.location.lastData.longitude);
            if (skipSamples > 0)
            {
                Debug.Log(TAG + "skipped");
                skipSamples--;
            }
            else
            {
                Debug.Log(TAG + " callgps ");

                OnGpsUpdated();
            }
        }
    }
    public void OnGpsUpdated()
    {
        latSamples.Add(Input.location.lastData.latitude);
        lonSamples.Add(Input.location.lastData.longitude);

        _avgLat = latSamples.Average();
        _avgLon = lonSamples.Average();

        if (GpsUpdated != null)
        {
            Debug.Log(TAG + " eventz before calls");

            GpsUpdated(Input.location.lastData.latitude, Input.location.lastData.longitude);
            GpsUpdatedPhase2();

            Debug.Log(TAG + " eventz after calls");


        }
    }

    public void EmulateGps()
    {
            Debug.Log(TAG + " eventz before calls");
            GpsUpdated(emulatorLat, emulatorLon);
            GpsUpdatedPhase2();
            emulatorLon -= 0.00003f;
            Debug.Log(TAG + " eventz after calls");
    }

    public void switchGPS(bool val)
    {
        gpsOn = val;
    }
}
