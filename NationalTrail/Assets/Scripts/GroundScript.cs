using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundScript : MonoBehaviour
{
    public GpsScript gpsScript;
    public Transform camTransform;
    public GameObject groundSamplePrefab;
    public List<GameObject> groundSamples { get { return _groundSamples; } }

    private List<GameObject> _groundSamples;
    private void Awake()
    {
        _groundSamples = new List<GameObject>();
    }
    // Start is called before the first frame update
    void Start()
    {
        gpsScript.GpsUpdatedSetMap += OnGpsUpdated;
    }

    public void OnGpsUpdated(double lat, double lon, float acc)
    {
        GameObject samp = Instantiate(groundSamplePrefab, new Vector3(camTransform.position.x, 0, camTransform.position.z), Quaternion.identity);
        _groundSamples.Add(samp);
    }
}