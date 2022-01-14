using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PoiDetectionScript : MonoBehaviour
{
    public MapScript map;
    private void OnTriggerEnter(Collider collider)
    {
        map.OnCamTriggeredPoiEnter(collider);
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "enter");
    }
    private void OnTriggerStay(Collider other)
    {
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "stay");

    }
    private void OnTriggerExit(Collider other)
    {
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "exit");

    }
}
