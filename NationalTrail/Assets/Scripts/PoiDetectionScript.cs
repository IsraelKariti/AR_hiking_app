using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class PoiDetectionScript : MonoBehaviour
{
    public MapScript map;
    private Vector3 globalEnterPosition;
    private Vector3 localEnterPosition;
    private Vector2 localEnterPositionV2;

    private Vector3 globalExitPosition;
    private Vector3 localExitPosition;
    private Vector2 localExitPositionV2;

    private Vector3 colliderMiddlePosition;
    //public Text t;
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
            Transform colliderTransform = collider.transform;
            localExitPosition = colliderTransform.InverseTransformPoint(globalExitPosition);
            localExitPositionV2 = new Vector2(localExitPosition.x, localExitPosition.z);

            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "exit: " + collider.gameObject + "\nglobal: " + globalExitPosition + "     local: " + localExitPosition + "\n\n============\n\n");
            // check if the phone camera was walking parallel to the connector
            // (the xz diff of enter and exit points are less than 1 meter) (connector local y is the axis that the user is walking through)
            if (Vector2.Distance(localEnterPositionV2, localExitPositionV2) < 1)
            {
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "parallel\n\n============\n\n");
                colliderMiddlePosition = colliderTransform.position;
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "colliderMiddlePosition"+ colliderMiddlePosition+"\n");

                // check if the parralel line is less than 3 meters from the map toppings in the global XZ plane
                Vector2 parallelShift = checkDistanceBetweenAdjacentToConnector();
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "parallelShift: " + parallelShift + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "parallelShift.sqrMagnitude: " + parallelShift.sqrMagnitude + "\n");

                // check if the magnitude is less than 3 (squared magnitude is less than 9) 
                if (parallelShift.sqrMagnitude<Values.GPS_ERROR_RADIUS_SQRD)
                    collider.transform.parent.GetComponent<MapToppingsScript>().OnUserWalkedParallelToConnector(parallelShift);
            }

        }
    }

    // check if the user walked parallel to the connector is also close enough to the connector to be considered a gps error
    private Vector2 checkDistanceBetweenAdjacentToConnector()
    {
        // take the XZ value of the middle parallel line point
        Vector3 middleParallel = (globalEnterPosition + globalExitPosition) / 2;
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "middleParallel" + middleParallel + "\n");
        Vector2 middleParallelXZ = new Vector2(middleParallel.x, middleParallel.z);
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "middleParallelXZ" + middleParallelXZ + "\n");

        // take the connector XZ middle point
        Vector2 connectorXZ = new Vector2(colliderMiddlePosition.x, colliderMiddlePosition.z);
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "connectorXZ" + connectorXZ + "\n");

        // compare
        return middleParallelXZ - connectorXZ ;
    }

    //private void Update()
    //{
    //    t.text = "" + transform.position;
    //}
}
