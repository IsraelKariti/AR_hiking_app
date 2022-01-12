using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareSightDist : IComparer<GameObject>
{
    public int Compare(GameObject go1, GameObject go2)
    {
        float dist1 = Vector3.Distance(go1.GetComponent<SightScript>().arCam.transform.position, go1.transform.position);
        float dist2 = Vector3.Distance(go2.GetComponent<SightScript>().arCam.transform.position, go2.transform.position);
        return (int)(dist1 - dist2);
    }
}
