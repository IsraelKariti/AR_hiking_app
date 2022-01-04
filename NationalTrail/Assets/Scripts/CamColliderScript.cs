
using UnityEngine;

public class CamColliderScript : MonoBehaviour
{
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("gooy collision enter");
    }
    public void OnCollisionStay(Collision collision)
    {
        Debug.Log("gooy collision stay");

    }
    public void OnCollisionExit(Collision collision)
    {
        Debug.Log("gooy collision exit");

    }
}
