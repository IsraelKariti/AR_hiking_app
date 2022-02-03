using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimberScript : MonoBehaviour
{
    private double lat;
    private double lon;
    private float altL;
    private float altH;

    public double Lat { get => lat; set => lat = value; }
    public double Lon { get => lon; set => lon = value; }
    public float AltL { get => altL; set => altL = value; }
    public float AltH { get => altH; set => altH = value; }

}
