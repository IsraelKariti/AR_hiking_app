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
    public LeastSquareScript leastSquareScript;
    public Text insideIndicator;
    private Vector3 enterPositionGlobal;
    private Vector3 enterPositionInConnector;
    private Vector2 enterPositionInConnectorV2;

    private Vector3 exitPositionGlobal;
    private Vector3 exitPositionInConnector;
    private Vector2 exitPositionInConnectorV2;

    private Vector3 colliderPositionGlobal;

    private GameObject currEnteredConnector;
    
    // this entire game object is only enabled by the LS script, after the map has been stable in the last 3 gps samples
    private void Start()
    {
        File.Delete(Application.persistentDataPath + "/collision.txt");
        File.Delete(Application.persistentDataPath + "/hitTurn.txt");
    }
    private void OnTriggerEnter(Collider collider)
    {
        // don't bother about entering a collider if you are already inside another collider
        if (collider.gameObject.tag == "poiConnector" && currEnteredConnector == null ) 
        {
            // disable the LS script to avoid movement in the map
            leastSquareScript.EnableLS = false;
            enterPositionGlobal = transform.position;
            enterPositionInConnector = collider.transform.InverseTransformPoint(enterPositionGlobal);
            enterPositionInConnectorV2 = new Vector2(enterPositionInConnector.x, enterPositionInConnector.z);
            currEnteredConnector = collider.gameObject;
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "\n\n\n\n\n" + DateTime.Now + " " + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "enter: " + collider.gameObject + " collider global pos:"+collider.transform.position+ "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "entor pos global: " + enterPositionGlobal + "   enter pos in connector: " + enterPositionInConnector + "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "enterPositionInConnectorV2: " + enterPositionInConnectorV2 + "\n");
            insideIndicator.text = "I";
        }
        if (collider.gameObject.tag.CompareTo("turn") == 0)
        {
            File.AppendAllText(Application.persistentDataPath + "/hitTurn.txt", "hit\n");
            // this will make map toppings script check if the toppings is locked horizontally before actually locking vertically
            mapToppingsScript.OnCamTriggeredPoiEnter(collider);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // this exit is only relevent when leaving the connector that has been entered originally
        if (collider.gameObject.tag == "poiConnector" && GameObject.ReferenceEquals( collider.gameObject, currEnteredConnector) )
        {
            exitPositionGlobal = transform.position;
            Transform colliderTransform = collider.transform;
            exitPositionInConnector = colliderTransform.InverseTransformPoint(exitPositionGlobal);
            exitPositionInConnectorV2 = new Vector2(exitPositionInConnector.x, exitPositionInConnector.z);
            float diffConnectorXZ = Vector2.Distance(enterPositionInConnectorV2, exitPositionInConnectorV2);// check if the user has walked parallel
            float diffConnectorY = Mathf.Abs(enterPositionInConnector.y - exitPositionInConnector.y);// check if the user has walked at least 2 meters in the direction of the connector
            
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "" + DateTime.Now + " " + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "exit: " + collider.gameObject + " collider global pos: " +  collider.transform.position+ "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "entor pos global: " + exitPositionGlobal + "   enter pos in connector: " + exitPositionInConnector + "\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "diffConnectorXZ: " + diffConnectorXZ+"\n");
            File.AppendAllText(Application.persistentDataPath + "/collision.txt", "diffConnectorY: " + diffConnectorY+"\n");

            // check if the user is moving parallel to a connector
            if (diffConnectorXZ < Values.ENTER_EXIT_DIFF_XZ_PARALLEL && diffConnectorY > Values.ENTER_EXIT_DIFF_Y_PARALLEL)
            {
                // check if the parralel line is less than 3 meters from the map toppings in the global XZ plane
                colliderPositionGlobal = colliderTransform.position;
                Vector2 shiftGlobalXZ = getGlobalShift();
                Vector3 shiftGlobalXYZ = new Vector3(shiftGlobalXZ.x, 0, shiftGlobalXZ.y);
                Vector3 prevLocalShiftInMapXYZ = map.transform.GetChild(0).localPosition;
                Vector2 prevLocalShiftInMapXZ = new Vector2(prevLocalShiftInMapXYZ.x, prevLocalShiftInMapXYZ.z);
                Vector3 localShiftInMapXYZ = map.transform.InverseTransformDirection(shiftGlobalXYZ);
                Vector2 localShiftInMapXZ = new Vector2(localShiftInMapXYZ.x, localShiftInMapXYZ.z);
                Vector2 combinedLocalShiftInMapXZ = localShiftInMapXZ + prevLocalShiftInMapXZ;

                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "is parallel\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "colliderPositionGlobal" + colliderPositionGlobal + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "shiftGlobalXZ" + shiftGlobalXZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "shiftGlobalXZ.sqrMagnitude: " + shiftGlobalXZ.sqrMagnitude + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "localShiftInMapXZ" + localShiftInMapXZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "prevLocalShiftInMapXZ" + prevLocalShiftInMapXZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "combinedLocalShiftInMap: " + combinedLocalShiftInMapXZ + "\n");
                File.AppendAllText(Application.persistentDataPath + "/collision.txt", "combinedLocalShiftInMap.sqrMagnitude: " + combinedLocalShiftInMapXZ.sqrMagnitude + "\n");

                collider.transform.parent.GetComponent<MapToppingsScript>().OnUserWalkedParallelToConnector(localShiftInMapXZ);
            }
            // after the shift has finished reenable the LS script
            leastSquareScript.EnableLS = true;
            // this will unlock the enter-exit procedure
            currEnteredConnector = null;

            insideIndicator.text = "O";
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
