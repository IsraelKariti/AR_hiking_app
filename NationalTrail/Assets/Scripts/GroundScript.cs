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
        gpsScript.GpsUpdated += OnGpsUpdated;
    }

    public void OnGpsUpdated(float lat, float lon)
    {
        Debug.Log(" eventz GroundScript OnGpsUpdated enter");

        GameObject samp = Instantiate(groundSamplePrefab, new Vector3(camTransform.position.x, 0, camTransform.position.z), Quaternion.identity);
        samp.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        _groundSamples.Add(samp);
        Debug.Log(" eventz GroundScript OnGpsUpdated end");

    }
}