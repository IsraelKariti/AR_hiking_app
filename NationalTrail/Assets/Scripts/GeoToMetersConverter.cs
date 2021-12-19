using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
//At the equator for longitude and for latitude anywhere, the following approximations are valid:
//1° = 111 km(or 60 nautical miles)
//0.1° = 11.1 km
//0.01° = 1.11 km(2 decimals, km accuracy)
//0.001° = 111 m
//0.0001° = 11.1 m
//0.00001° = 1.11 m 
//0.000001° = 0.11 m(7 decimals, cm accuracy)

//Earth is a sphere with a circumference of 40075 km == 40075000 m == 4007500000 cm
//Length in meters of 1° of latitude = always 111.3194444 km = 40075 km / 360
//Length in meters of 1° of longitude = 40075 km * cos( latitude ) / 360
// lat 1 m = 0.000008983156581 degrees

// lon 1 m = 0.000006273659336 degrees = cos(33.312090)*lat  = 0.8356914933 (QIRYAT-SHMONA)
// lon 1 m = 0.000007678791061 degrees = cos(31.2624908)*lat = 0.8547987549 (BEER-SHEVA)
// lon 1 m = 0.000007813607031 degrees = cos(29.563851)*lat  = 0.8698063938 (EILAT)
public class GeoToMetersConverter
{
    private const double oneLatAngleInMeters = 111319.4444;
    // I have reverse engineered this parameter from the Be'er Sheva GIS service and estimated it to be:
    //private const double oneLatAngleInMeters = 110872.8482;
    // but actually it doesn't really matter that much: 
    // the difference between the constants get cancelled out, 
    // that's because the diff is used to set both the Pois and the samples on the map,
    // and both of them have lat-lan close to each other, that is why they will end up as neighbours on the map,
    // they are at the same distance from the center
    // However, making the map gameobject huge causes some disturbance,
    // probably due to Unity's under the hood math, for example: rounding errors
    // so it's better to work with small tiles

    public static double convertLatDiffToMeters(double LatDiff)
    {
        return LatDiff * oneLatAngleInMeters;
    }
    public static double convertLonDiffToMeters(double LonDiff, double lat)
    {
        return LonDiff * Math.Cos(lat*(Math.PI / 180) ) * oneLatAngleInMeters;
    }
    
}
