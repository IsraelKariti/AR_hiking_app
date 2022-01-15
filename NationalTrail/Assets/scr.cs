using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr : MonoBehaviour
{
    public GameObject go1;
    public GameObject go2;
    public GameObject go3;
    // Start is called before the first frame update
    void Start()
    {
        go3.GetComponent<PoiConnectorScript>().positionInMap(go1, go2);
 
    }

}
