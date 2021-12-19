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

    public float avgLat { get { return _avgLat; } }
    public float avgLon { get { return _avgLon; } }
    
    public delegate void GpsUpdatedEventHandler(float lat, float lon);
    public event GpsUpdatedEventHandler GpsUpdated;
    //private float emulatorLat = 31.26255f;
    //private float emulatorLon = 34.79350f;
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
        Input.location.Start(0,0);
        latSamples = new List<float>();
        lonSamples = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.location.status == LocationServiceStatus.Running && Input.location.lastData.timestamp > prevTimeStamp )
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
            Debug.Log(TAG + " altitude : "+Input.location.lastData.altitude+ " horizontal accuracy: "+Input.location.lastData.horizontalAccuracy+ " vertical accuracy: "+ Input.location.lastData.verticalAccuracy);

            GpsUpdated(Input.location.lastData.latitude, Input.location.lastData.longitude);
            //GpsUpdated(emulatorLat, emulatorLon);
            //emulatorLon -= 0.00003f;
            //Debug.Log(TAG + "Limit samples AFTER" + limitSamples);

            
        }
    }
}
