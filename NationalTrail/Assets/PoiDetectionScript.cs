using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoiDetectionScript : MonoBehaviour
{
    public MapScript map;
    private void OnTriggerEnter(Collider other)
    {
        map.OnCamTriggeredPoi(other);
    }
}
