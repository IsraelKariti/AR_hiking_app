using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    
    // listener for the map+ground
    public delegate void GpsUpdatedSetSampleEventHandler(float lat, float lon, float acc);
    public event GpsUpdatedSetSampleEventHandler GpsUpdatedSetMap;
    public delegate void GpsUpdatedLeastSquaresEventHandler();
    public event GpsUpdatedLeastSquaresEventHandler GpsUpdatedCalcLeastSquares;
    public bool gpsOn { set { _gpsOn = value; } }
    private float emulatorLat = 31.26255f;
    private float emulatorLon = 34.79350f;
    private float inLat;// the input location lat 
    private float inLon;
    private float inAcc;
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
        File.Delete(Application.persistentDataPath + "/coords.txt");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.location.status == LocationServiceStatus.Running && Input.location.lastData.timestamp > prevTimeStamp && _gpsOn && Input.location.lastData.horizontalAccuracy<4.0f)
        {
            inLat = Input.location.lastData.latitude;
            inLon = Input.location.lastData.longitude;
            inAcc = Input.location.lastData.horizontalAccuracy;

            prevTimeStamp = Input.location.lastData.timestamp;
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
        
        latSamples.Add(inLat);
        lonSamples.Add(inLon);

        _avgLat = latSamples.Average();
        _avgLon = lonSamples.Average();

        
            GpsUpdatedSetMap(inLat, inLon, inAcc);
            GpsUpdatedCalcLeastSquares();

            File.AppendAllText(Application.persistentDataPath + "/coords.txt", "lat: " + inLat + " lon: " + inLon + " acc: "+inAcc+"\n");
        
    }

    public void EmulateGps()
    {
            GpsUpdatedSetMap(inLat, inLon, inAcc);
            GpsUpdatedCalcLeastSquares();
            File.AppendAllText(Application.persistentDataPath + "/coords.txt", "lat: " + emulatorLat + " lon: " + emulatorLon + " acc: " + inAcc + "\n");

        emulatorLon -= 0.00003f;
    }

    public void switchGPS(bool val)
    {
        gpsOn = val;
    }
}
