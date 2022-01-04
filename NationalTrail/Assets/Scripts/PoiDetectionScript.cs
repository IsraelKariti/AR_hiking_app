using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoiDetectionScript : MonoBehaviour
{
    public MapScript map;
    private void OnTriggerEnter(Collider collider)
    {
        map.OnCamTriggeredPoiEnter(collider);

    }
}
