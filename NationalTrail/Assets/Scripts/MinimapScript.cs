using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    public Camera cam;


    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(cam.transform.position.x, 20, cam.transform.position.z);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, cam.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}
