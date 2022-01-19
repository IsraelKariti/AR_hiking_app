using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Values
{
    public static float GPS_ERROR_RADIUS = 4;
    public static float GPS_ERROR_RADIUS_SQRD = GPS_ERROR_RADIUS*GPS_ERROR_RADIUS;
    public static int SKIP_SAMPLES = 1;
    public static float ENTER_EXIT_DIFF_XZ_PARALLEL = 1f; // the maximal distance between enter and exit points in collider that define a parallel line

    public static int MIN_GPS_SAMPLES_FOR_TOPPINGS_SHIFT = 10;// this is the minimal gps samples that are used for LS that needs to be acquired before the shifting of the toppings begins 
}
