using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class PoiDetectionScript : MonoBehaviour
{
    public MapScript map;
    public MapToppingsScript mapToppingsScript;
    private Vector3 enterPositionGlobal;
    private Vector3 enterPositionInConnector;
    private Vector2 enterPositionInConnectorV2;

    private Vector3 exitPositionGlobal;
    private Vector3 exitPositionInConnector;
    private Vector2 exitPositionInConnectorV2;

    private Vector3 colliderPositionGlobal;
    //public Text t;
    private void Start()
    {
        File.Delete(Application.persistentDataPath + "/collision.txt");
        File.Delete(Application.persistentDataPath + "/hitTurn.txt");
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "poiConnector")
        {
            enterPositionGlobal = transform.position;
            enterPositionInConnector = collider.transform.InverseTransformPoint(enterPositionGlobal);
            enterPositionInConnectorV2 = new Vector2(enterPositionInConnector.x, enterPositionInConnector.z);
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "\n\n\n\n\nenter: " + collider.gameObject + "\nglobal: " + enterPositionGlobal + "   local: " + enterPositionInConnector + "\n\n");
        }
        if (collider.gameObject.tag == "turn")
        {
            File.AppendAllText(Application.persistentDataPath + "/hitTurn.txt", "hit\n");
            mapToppingsScript.OnCamTriggeredPoiEnter(collider);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "poiConnector")
        {
            exitPositionGlobal = transform.position;
            Transform colliderTransform = collider.transform;
            exitPositionInConnector = colliderTransform.InverseTransformPoint(exitPositionGlobal);
            exitPositionInConnectorV2 = new Vector2(exitPositionInConnector.x, exitPositionInConnector.z);

            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "" + DateTime.Now + "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "exit: " + collider.gameObject + "\nglobal: " + exitPositionGlobal + "     local: " + exitPositionInConnector + "\n\n");
            // check if the phone camera was walking parallel to the connector
            // (the xz diff of enter and exit points are less than 1 meter) (connector local y is the axis that the user is walking through)
            float diffConnectorXZ = Vector2.Distance(enterPositionInConnectorV2, exitPositionInConnectorV2);
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "diffConnectorXZ: " + diffConnectorXZ+"\n");

            // check if the user is moving parallel to a connector
            if (diffConnectorXZ < Values.ENTER_EXIT_DIFF_XZ_PARALLEL)
            {
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "is parallel\n");
                colliderPositionGlobal = colliderTransform.position;
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "colliderPositionGlobal" + colliderPositionGlobal + "\n");

                // check if the parralel line is less than 3 meters from the map toppings in the global XZ plane
                Vector2 shiftGlobalXZ = getGlobalShift();
                Vector3 shiftGlobalXYZ = new Vector3(shiftGlobalXZ.x, 0, shiftGlobalXZ.y);
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "shiftGlobalXZ" + shiftGlobalXZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "shiftGlobalXYZ" + shiftGlobalXYZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "shiftGlobalXZ.sqrMagnitude: " + shiftGlobalXZ.sqrMagnitude + "\n");
                Vector3 localShiftInMapXYZ = map.transform.InverseTransformDirection(shiftGlobalXYZ);
                Vector2 localShiftInMapXZ = new Vector2(localShiftInMapXYZ.x, localShiftInMapXYZ.z);
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "localShiftInMapXYZ" + localShiftInMapXYZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "localShiftInMapXZ" + localShiftInMapXZ + "\n");
                Vector3 prevLocalShiftInMapXYZ = map.transform.GetChild(0).localPosition;
                Vector2 prevLocalShiftInMapXZ = new Vector2(prevLocalShiftInMapXYZ.x, prevLocalShiftInMapXYZ.z);
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "prevLocalShiftInMapXYZ" + prevLocalShiftInMapXYZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "prevLocalShiftInMapXZ" + prevLocalShiftInMapXZ + "\n");

                Vector2 combinedLocalShiftInMapXZ = localShiftInMapXZ + prevLocalShiftInMapXZ;
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "combinedLocalShiftInMap: " + combinedLocalShiftInMapXZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "combinedLocalShiftInMap.sqrMagnitude: " + combinedLocalShiftInMapXZ.sqrMagnitude + "\n");

                collider.transform.parent.GetComponent<MapToppingsScript>().OnUserWalkedParallelToConnector(localShiftInMapXZ);

            }

        }
    }

    // get the global direction on the XZ plane that the collider+toppings should move
    private Vector2 getGlobalShift()
    {

        // take the connector XZ middle point
        Vector2 colliderGlobalXZ = new Vector2(colliderPositionGlobal.x, colliderPositionGlobal.z);
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "colliderGlobalXZ" + colliderGlobalXZ + "\n");

        // take the XZ value of the middle parallel line point
        Vector3 middleParallelGlobal = (enterPositionGlobal + exitPositionGlobal) / 2;
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "middleParallelGlobal" + middleParallelGlobal + "\n");
        Vector2 middleParallelGlobalXZ = new Vector2(middleParallelGlobal.x, middleParallelGlobal.z);
        File.AppendAllText(Application.persistentDataPath + "/collision.txt", "middleParallelGlobalXZ" + middleParallelGlobalXZ + "\n");

        

        // compare
        return middleParallelGlobalXZ - colliderGlobalXZ;
    }


}
