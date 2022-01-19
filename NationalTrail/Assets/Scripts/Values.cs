using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Values
{
    public static float GPS_ERROR_RADIUS = 4;
    public static float GPS_ERROR_RADIUS_SQRD = GPS_ERROR_RADIUS*GPS_ERROR_RADIUS;
    public static int SKIP_SAMPLES = 10;
    public static float ENTER_EXIT_DIFF_XZ_PARALLEL = 1f; // the maximal distance between enter and exit points in collider that define a parallel line

    public static int MIN_GPS_SAMPLES_FOR_TOPPINGS_SHIFT = 3;
}
