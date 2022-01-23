using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Values
{
    public static float GPS_ERROR_RADIUS = 10;
    public static float GPS_ERROR_RADIUS_SQRD = GPS_ERROR_RADIUS*GPS_ERROR_RADIUS;
    public static float CONNECTOR_COLLIDER_DIAMETER = GPS_ERROR_RADIUS*2;

    public static int SKIP_SAMPLES = 5;
    public static float ENTER_EXIT_DIFF_XZ_PARALLEL = 1f; // the maximal distance between enter and exit points in collider that define a parallel line

    public static int MIN_THRESHOLD_ROTATION_Y_CONSIDERED_STABLE = 1;// if the map is rotating under 1 degree than it is stable
    public static float MIN_THRESHOLD_REPOSITION_X_CONSIDERED_STABLE = 1f;
    public static float MIN_THRESHOLD_REPOSITION_Z_CONSIDERED_STABLE = 1f;
    public static int MIN_GPS_SAMPLES_TO_CONSTANT_MAP_FOR_STABILITY = 7;// the minimal count of gps samples that didn't change the position of the map

    public static float ENTER_EXIT_DIFF_Y_PARALLEL = 2; // the minimum amount of meters that a user has to walk parllel to a connector for it to be considered as walking parallel (ex. 20 cm doesn't count)
    public static float MIN_THRESHOLD_SHIFT = 0f; // the minimum meters for toppings shift 
    public static float MIN_THRESHOLD_SHIFT_SQRD = MIN_THRESHOLD_SHIFT*MIN_THRESHOLD_SHIFT; // the minimum meters for toppings shift SQUARED
}
