using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSphereToggleSCript : MonoBehaviour
{
    public Material mat;

    public void toggleSphere(bool b)
    {
        if (b)
        {
            mat.color = new Color(1, 1, 1, 1);
        }
        else
        {
            mat.color = new Color(1, 1, 1, 0);

        }
    }
}
