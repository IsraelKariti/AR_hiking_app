using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PoiDetectionScript : MonoBehaviour
{
    public MapScript map;
    private Vector3 globalEnterPosition;
    private Vector3 localEnterPosition;
    private Vector2 localEnterPositionV2;

    private Vector3 globalExitPosition;
    private Vector3 localExitPosition;
    private Vector2 localExitPositionV2;
    public Text t;
    private void Start()
    {
        File.Delete(Application.persistentDataPath + "/collision.txt");
    }
    private void OnTriggerEnter(Collider collider)
    {

        //map.OnCamTriggeredPoiEnter(collider);
        if (collider.gameObject.tag == "poiConnector")
        {
            globalEnterPosition = transform.position;
            localEnterPosition = collider.transform.InverseTransformPoint(globalEnterPosition);
            localEnterPositionV2 = new Vector2(localEnterPosition.x, localEnterPosition.z);
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "enter: " + collider.gameObject + "\nglobal: " + globalEnterPosition + "   local: " + localEnterPosition + "\n\n");
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "poiConnector")
        {
            globalExitPosition = transform.position;
            localExitPosition = collider.transform.InverseTransformPoint(globalExitPosition);
            localExitPositionV2 = new Vector2(localExitPosition.x, localExitPosition.z);

            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "exit: " + collider.gameObject + "\nglobal: " + globalExitPosition + "     local: " + localExitPosition + "\n\n============\n\n");
            // check if the phone camera was walking parallel to the connector
            if (Vector2.Distance(localEnterPositionV2, localExitPositionV2) < 1)
            {
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "parallel\n\n============\n\n");

            }

        }
    } 
    private void Update()
    {
        t.text = "" + transform.position;
    }
}
